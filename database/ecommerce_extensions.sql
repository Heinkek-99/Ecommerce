-- ============================================================
-- Extensions e-commerce — Promotions · Multi-devise · Notifications
-- Dialecte : PostgreSQL  |  Dépend de : ecommerce_schema.sql
-- ============================================================

-- ============================================================
-- MODULE 1 : CAMPAGNES & PROMOTIONS (dont Black Friday)
-- ============================================================

-- Une campagne regroupe des promotions et des ventes flash.
-- Le champ `type` distingue : 'evergreen' | 'seasonal' | 'flash' | 'black_friday'
-- Le champ `priority` permet de résoudre les conflits de cumul.
CREATE TABLE campaigns (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name       TEXT        NOT NULL,
    -- 'evergreen' | 'seasonal' | 'flash' | 'black_friday'
    type       TEXT        NOT NULL DEFAULT 'seasonal'
               CHECK (type IN ('evergreen','seasonal','flash','black_friday')),
    starts_at  TIMESTAMPTZ NOT NULL,
    ends_at    TIMESTAMPTZ NOT NULL,
    is_active  BOOLEAN     NOT NULL DEFAULT TRUE,
    -- Priorité de la campagne (plus élevé = appliqué en premier)
    priority   INT         NOT NULL DEFAULT 0,
    CONSTRAINT valid_campaign_window CHECK (ends_at > starts_at)
);

-- Une promotion peut être :
--   • un code promo classique (ex. SUMMER20)
--   • une réduction automatique sans code
--   • une offre Black Friday (rattachée à une campagne de type black_friday)
--
-- discount_type : 'percentage' | 'fixed_amount' | 'free_shipping' | 'bxgy' (buy X get Y)
CREATE TABLE promotions (
    id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    campaign_id       UUID        REFERENCES campaigns(id) ON DELETE SET NULL,
    -- NULL = réduction automatique, pas besoin de saisir de code
    code              TEXT        UNIQUE,
    -- 'percentage' | 'fixed_amount' | 'free_shipping' | 'bxgy'
    discount_type     TEXT        NOT NULL
                      CHECK (discount_type IN ('percentage','fixed_amount','free_shipping','bxgy')),
    discount_value    NUMERIC(10,4) NOT NULL DEFAULT 0 CHECK (discount_value >= 0),
    min_order_amount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    -- NULL = utilisations illimitées
    max_uses          INT         CHECK (max_uses > 0),
    -- Compteur d'utilisations (incrémenté via trigger ou applicatif)
    uses_count        INT         NOT NULL DEFAULT 0,
    -- max_uses_per_user : 1 = usage unique par utilisateur
    max_uses_per_user INT         NOT NULL DEFAULT 1 CHECK (max_uses_per_user > 0),
    -- TRUE = peut se cumuler avec d'autres promos de moindre priorité
    stackable         BOOLEAN     NOT NULL DEFAULT FALSE,
    expires_at        TIMESTAMPTZ,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Cibles d'une promotion (polymorphique) :
-- target_type : 'product' | 'variant' | 'category' | 'seller' | 'cart' (toujours applicable)
CREATE TABLE promotion_targets (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    promotion_id  UUID NOT NULL REFERENCES promotions(id) ON DELETE CASCADE,
    -- 'product' | 'variant' | 'category' | 'seller' | 'cart'
    target_type   TEXT NOT NULL CHECK (target_type IN ('product','variant','category','seller','cart')),
    -- NULL si target_type = 'cart'
    target_id     UUID,
    CONSTRAINT target_coherent CHECK (
        (target_type = 'cart' AND target_id IS NULL) OR
        (target_type <> 'cart' AND target_id IS NOT NULL)
    )
);

-- Historique d'utilisation : une ligne par (promotion × commande).
-- Permet de vérifier max_uses_per_user côté applicatif ou via constraint.
CREATE TABLE promotion_uses (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    promotion_id     UUID          NOT NULL REFERENCES promotions(id),
    user_id          UUID          NOT NULL REFERENCES users(id),
    order_id         UUID          NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    discount_applied NUMERIC(12,2) NOT NULL CHECK (discount_applied >= 0),
    used_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    UNIQUE (promotion_id, order_id)
);

-- Ventes flash : prix spécial sur une variante, stock limité, fenêtre horaire courte.
-- Idéal pour les opérations type Black Friday (ex. 500 unités à -60% pendant 2h).
CREATE TABLE flash_sales (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    campaign_id UUID        REFERENCES campaigns(id) ON DELETE SET NULL,
    variant_id  UUID        NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    sale_price  NUMERIC(12,2) NOT NULL CHECK (sale_price >= 0),
    -- NULL = pas de limite de stock dédiée (utilise le stock normal)
    stock_limit INT         CHECK (stock_limit > 0),
    sold_count  INT         NOT NULL DEFAULT 0,
    starts_at   TIMESTAMPTZ NOT NULL,
    ends_at     TIMESTAMPTZ NOT NULL,
    CONSTRAINT valid_flash_window CHECK (ends_at > starts_at),
    CONSTRAINT sold_within_limit  CHECK (stock_limit IS NULL OR sold_count <= stock_limit)
);

-- Index
CREATE INDEX idx_campaigns_active    ON campaigns(is_active, starts_at, ends_at);
CREATE INDEX idx_promotions_code     ON promotions(code) WHERE code IS NOT NULL;
CREATE INDEX idx_promotions_campaign ON promotions(campaign_id);
CREATE INDEX idx_promo_uses_user     ON promotion_uses(promotion_id, user_id);
CREATE INDEX idx_flash_sales_variant ON flash_sales(variant_id);
CREATE INDEX idx_flash_sales_window  ON flash_sales(starts_at, ends_at);

-- Vue : prix effectif d'une variante (flash sale prioritaire si active)
CREATE VIEW v_effective_price AS
SELECT
    pv.id           AS variant_id,
    pv.product_id,
    COALESCE(pv.price, p.base_price) AS base_price,
    fs.sale_price   AS flash_price,
    CASE
        WHEN fs.id IS NOT NULL
             AND NOW() BETWEEN fs.starts_at AND fs.ends_at
             AND (fs.stock_limit IS NULL OR fs.sold_count < fs.stock_limit)
        THEN fs.sale_price
        ELSE COALESCE(pv.price, p.base_price)
    END             AS effective_price
FROM product_variants pv
JOIN products p ON p.id = pv.product_id
LEFT JOIN flash_sales fs ON fs.variant_id = pv.id;

-- ============================================================
-- MODULE 2 : GESTION MULTI-DEVISE
-- ============================================================

-- Référentiel de devises actives (ISO 4217).
CREATE TABLE currencies (
    code      TEXT PRIMARY KEY,         -- ex. 'EUR', 'USD', 'GBP', 'JPY'
    name      TEXT NOT NULL,
    symbol    TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- Taux de change horodatés. Un job cron insère une nouvelle ligne
-- à chaque rafraîchissement (ex. toutes les heures via API openexchangerates).
-- La devise de référence (base) est définie en configuration applicative.
CREATE TABLE exchange_rates (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    from_currency TEXT        NOT NULL REFERENCES currencies(code),
    to_currency   TEXT        NOT NULL REFERENCES currencies(code),
    rate          NUMERIC(18,8) NOT NULL CHECK (rate > 0),
    fetched_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT different_currencies CHECK (from_currency <> to_currency)
);

CREATE INDEX idx_exchange_rates_pair ON exchange_rates(from_currency, to_currency, fetched_at DESC);

-- Vue : dernier taux connu pour chaque paire
CREATE VIEW v_latest_exchange_rates AS
SELECT DISTINCT ON (from_currency, to_currency)
    from_currency,
    to_currency,
    rate,
    fetched_at
FROM exchange_rates
ORDER BY from_currency, to_currency, fetched_at DESC;

-- Modification de la table orders pour supporter la multi-devise.
-- On stocke :
--   • currency_code  : devise dans laquelle l'acheteur a payé
--   • total_amount   : montant dans la devise de l'acheteur
--   • base_currency  : devise de référence interne (ex. 'EUR')
--   • total_in_base  : montant converti dans la devise de référence
--   • fx_rate_snapshot : taux appliqué au moment de la commande (immuable)
ALTER TABLE orders
    ADD COLUMN currency_code    TEXT        NOT NULL DEFAULT 'EUR' REFERENCES currencies(code),
    ADD COLUMN total_in_base    NUMERIC(12,2),
    ADD COLUMN base_currency    TEXT        NOT NULL DEFAULT 'EUR',
    ADD COLUMN fx_rate_snapshot NUMERIC(18,8);

-- Même enrichissement sur les paiements
ALTER TABLE payments
    ADD COLUMN currency_code TEXT REFERENCES currencies(code),
    ADD COLUMN amount_in_base NUMERIC(12,2),
    ADD COLUMN fx_rate        NUMERIC(18,8);

-- Données de référence : devises courantes
INSERT INTO currencies (code, name, symbol) VALUES
    ('EUR', 'Euro',                '€'),
    ('USD', 'US Dollar',           '$'),
    ('GBP', 'British Pound',       '£'),
    ('JPY', 'Japanese Yen',        '¥'),
    ('CHF', 'Swiss Franc',         'CHF'),
    ('CAD', 'Canadian Dollar',     'CA$'),
    ('AUD', 'Australian Dollar',   'A$'),
    ('MAD', 'Moroccan Dirham',     'MAD'),
    ('TND', 'Tunisian Dinar',      'TND')
ON CONFLICT (code) DO NOTHING;

-- ============================================================
-- MODULE 3 : NOTIFICATIONS (email · SMS · push)
-- ============================================================

-- Templates multilingues par canal et par événement.
-- event_type : 'order_confirmed' | 'order_shipped' | 'order_delivered' |
--              'promo_alert' | 'flash_sale_start' | 'password_reset' | etc.
-- channel    : 'email' | 'sms' | 'push'
-- body_template supporte les variables Mustache/Jinja : {{user_name}}, {{order_id}}, etc.
CREATE TABLE notification_templates (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_type    TEXT NOT NULL,
    -- 'email' | 'sms' | 'push'
    channel       TEXT NOT NULL CHECK (channel IN ('email','sms','push')),
    lang          TEXT NOT NULL DEFAULT 'fr',   -- code ISO 639-1
    -- Objet (email uniquement)
    subject       TEXT,
    body_template TEXT NOT NULL,
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,
    UNIQUE (event_type, channel, lang)
);

-- Préférences de notification par utilisateur.
-- Permet de respecter le RGPD (opt-out par canal).
CREATE TABLE user_notification_preferences (
    user_id        UUID    NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    channel        TEXT    NOT NULL CHECK (channel IN ('email','sms','push')),
    event_type     TEXT    NOT NULL,
    subscribed     BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (user_id, channel, event_type)
);

-- File de notifications à envoyer (ou déjà envoyées).
-- status : 'pending' | 'sending' | 'sent' | 'failed' | 'cancelled'
-- payload : données de rendu du template (JSON)
-- scheduled_at : permet l'envoi différé (ex. rappel panier 24h après abandon)
CREATE TABLE notifications (
    id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id      UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    template_id  UUID        REFERENCES notification_templates(id),
    -- Référence optionnelle vers la commande, la promo, etc.
    order_id     UUID        REFERENCES orders(id) ON DELETE SET NULL,
    channel      TEXT        NOT NULL CHECK (channel IN ('email','sms','push')),
    status       TEXT        NOT NULL DEFAULT 'pending'
                 CHECK (status IN ('pending','sending','sent','failed','cancelled')),
    -- Variables de rendu sérialisées en JSON : {"user_name": "Alice", "total": "49.90 €"}
    payload      JSONB       NOT NULL DEFAULT '{}',
    scheduled_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    sent_at      TIMESTAMPTZ,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Journal détaillé par tentative d'envoi (retry, erreurs fournisseur).
-- provider     : 'sendgrid' | 'mailjet' | 'twilio' | 'firebase' | etc.
-- provider_ref : identifiant de message retourné par le fournisseur
-- outcome      : 'delivered' | 'bounced' | 'spam' | 'unsubscribed' | 'error'
CREATE TABLE notification_logs (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    notification_id UUID        NOT NULL REFERENCES notifications(id) ON DELETE CASCADE,
    provider        TEXT        NOT NULL,
    provider_ref    TEXT,
    outcome         TEXT        CHECK (outcome IN ('delivered','bounced','spam','unsubscribed','error')),
    error_message   TEXT,
    logged_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Index
CREATE INDEX idx_notifications_user     ON notifications(user_id, status);
CREATE INDEX idx_notifications_schedule ON notifications(scheduled_at) WHERE status = 'pending';
CREATE INDEX idx_notif_logs_notif       ON notification_logs(notification_id);

-- ============================================================
-- DONNÉES DE RÉFÉRENCE : templates par défaut
-- ============================================================
INSERT INTO notification_templates (event_type, channel, lang, subject, body_template) VALUES
('order_confirmed', 'email', 'fr',
 'Votre commande {{order_id}} est confirmée',
 'Bonjour {{user_name}}, votre commande d''un montant de {{total}} a bien été reçue.'),

('order_shipped', 'email', 'fr',
 'Votre commande {{order_id}} est en route',
 'Bonjour {{user_name}}, votre colis a été expédié. Numéro de suivi : {{tracking_number}}.'),

('order_confirmed', 'sms', 'fr',
 NULL,
 'Commande {{order_id}} confirmée — {{total}}. Merci pour votre achat !'),

('flash_sale_start', 'push', 'fr',
 NULL,
 '{{campaign_name}} : -{{discount}}% sur {{product_name}} pendant {{duration}} seulement !'),

('promo_alert', 'email', 'fr',
 'Offre exclusive : {{discount}}% de réduction',
 'Bonjour {{user_name}}, utilisez le code {{promo_code}} avant le {{expires_at}}.'),

('order_confirmed', 'email', 'en',
 'Your order {{order_id}} is confirmed',
 'Hi {{user_name}}, your order of {{total}} has been received. Thank you!'),

('flash_sale_start', 'push', 'en',
 NULL,
 '{{campaign_name}}: {{discount}}% off {{product_name}} for {{duration}} only!')
ON CONFLICT (event_type, channel, lang) DO NOTHING;

-- ============================================================
-- FONCTION : créer une notification depuis un événement
-- ============================================================
CREATE OR REPLACE FUNCTION notify_user(
    p_user_id    UUID,
    p_event_type TEXT,
    p_channel    TEXT,
    p_payload    JSONB       DEFAULT '{}',
    p_order_id   UUID        DEFAULT NULL,
    p_lang       TEXT        DEFAULT 'fr',
    p_scheduled  TIMESTAMPTZ DEFAULT NOW()
)
RETURNS UUID LANGUAGE plpgsql AS $$
DECLARE
    v_template_id UUID;
    v_notif_id    UUID;
    v_subscribed  BOOLEAN;
BEGIN
    SELECT subscribed INTO v_subscribed
    FROM user_notification_preferences
    WHERE user_id = p_user_id
      AND channel = p_channel
      AND event_type = p_event_type;

    IF v_subscribed = FALSE THEN
        RETURN NULL;
    END IF;

    SELECT id INTO v_template_id
    FROM notification_templates
    WHERE event_type = p_event_type
      AND channel    = p_channel
      AND lang       = p_lang
      AND is_active  = TRUE
    LIMIT 1;

    INSERT INTO notifications (user_id, template_id, order_id, channel, payload, scheduled_at)
    VALUES (p_user_id, v_template_id, p_order_id, p_channel, p_payload, p_scheduled)
    RETURNING id INTO v_notif_id;

    RETURN v_notif_id;
END;
$$;
