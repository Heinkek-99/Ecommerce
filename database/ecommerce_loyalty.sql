-- ============================================================
-- MODULE FIDÉLITÉ — Points · Tiers · Récompenses
-- Dialecte : PostgreSQL  |  Dépend de : ecommerce_schema.sql
-- ============================================================

-- ============================================================
-- PROGRAMME DE FIDÉLITÉ
-- ============================================================

-- Un seul programme actif en règle générale, mais la table
-- permet d'en avoir plusieurs (ex. programme VIP séparé).
--   points_per_euro : points crédités par euro dépensé (ex. 10)
--   euro_per_point  : valeur d'un point en euro à la rédemption (ex. 0.01)
--   expiry_days     : durée de vie des points depuis leur gain (NULL = pas d'expiration)
CREATE TABLE loyalty_programs (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name             TEXT           NOT NULL,
    points_per_euro  NUMERIC(10,4)  NOT NULL DEFAULT 10  CHECK (points_per_euro > 0),
    euro_per_point   NUMERIC(10,6)  NOT NULL DEFAULT 0.01 CHECK (euro_per_point > 0),
    expiry_days      INT            CHECK (expiry_days > 0),
    is_active        BOOLEAN        NOT NULL DEFAULT TRUE,
    created_at       TIMESTAMPTZ    NOT NULL DEFAULT NOW()
);

-- ============================================================
-- NIVEAUX (TIERS)
-- ============================================================

-- Chaque tier définit :
--   min_points       : seuil de points à vie pour y accéder
--   bonus_multiplier : coefficient appliqué sur le gain de points (ex. 1.5 = +50%)
--   perks            : avantages libres en JSON (ex. {"free_shipping": true, "early_access": true})
--   rank             : ordre d'affichage (1 = le plus bas)
CREATE TABLE loyalty_tiers (
    id               UUID          PRIMARY KEY DEFAULT uuid_generate_v4(),
    program_id       UUID          NOT NULL REFERENCES loyalty_programs(id) ON DELETE CASCADE,
    name             TEXT          NOT NULL,                  -- ex. 'Bronze', 'Silver', 'Gold', 'Platinum'
    min_points       INT           NOT NULL DEFAULT 0 CHECK (min_points >= 0),
    bonus_multiplier NUMERIC(5,2)  NOT NULL DEFAULT 1.0 CHECK (bonus_multiplier >= 1),
    perks            JSONB         NOT NULL DEFAULT '{}',
    rank             INT           NOT NULL DEFAULT 1,
    UNIQUE (program_id, name),
    UNIQUE (program_id, rank)
);

-- ============================================================
-- COMPTES FIDÉLITÉ (1 par utilisateur × programme)
-- ============================================================

--   points_balance  : solde courant (peut diminuer après rédemption ou expiration)
--   points_lifetime : total cumulé à vie (ne diminue jamais — sert à calculer le tier)
CREATE TABLE loyalty_accounts (
    id               UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id          UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    program_id       UUID        NOT NULL REFERENCES loyalty_programs(id),
    current_tier_id  UUID        REFERENCES loyalty_tiers(id),
    points_balance   INT         NOT NULL DEFAULT 0 CHECK (points_balance >= 0),
    points_lifetime  INT         NOT NULL DEFAULT 0 CHECK (points_lifetime >= 0),
    tier_updated_at  TIMESTAMPTZ,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (user_id, program_id)
);

CREATE INDEX idx_loyalty_accounts_user ON loyalty_accounts(user_id);

-- ============================================================
-- TRANSACTIONS DE POINTS
-- ============================================================

-- Chaque mouvement de points est tracé ici (immuable — jamais d'UPDATE).
-- type : 'earn'      → achat validé
--        'bonus'     → campagne, parrainage, anniversaire, tier upgrade
--        'redeem'    → utilisation d'une récompense
--        'expire'    → expiration automatique
--        'adjust'    → correction manuelle par un admin
--        'refund'    → remboursement de points suite à retour commande
--
-- expires_at : uniquement renseigné pour les transactions de type 'earn' et 'bonus'
CREATE TABLE loyalty_transactions (
    id          UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_id  UUID        NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    order_id    UUID        REFERENCES orders(id) ON DELETE SET NULL,
    reward_id   UUID        REFERENCES loyalty_rewards(id) ON DELETE SET NULL,  -- si type='redeem'
    type        TEXT        NOT NULL
                CHECK (type IN ('earn','bonus','redeem','expire','adjust','refund')),
    -- Positif = crédit, négatif = débit
    points      INT         NOT NULL,
    description TEXT,
    expires_at  TIMESTAMPTZ,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_loyalty_tx_account  ON loyalty_transactions(account_id, created_at DESC);
CREATE INDEX idx_loyalty_tx_expiry   ON loyalty_transactions(expires_at)
    WHERE type IN ('earn','bonus') AND expires_at IS NOT NULL;

-- ============================================================
-- RÉCOMPENSES
-- ============================================================

-- reward_type : 'discount_fixed'    → réduction fixe (ex. 5 €)
--               'discount_percent'  → réduction en % (ex. 10 %)
--               'free_shipping'     → livraison offerte
--               'gift_product'      → produit offert (variant_id dans metadata)
--               'early_access'      → accès anticipé vente/collection
--
-- stock : NULL = illimité
CREATE TABLE loyalty_rewards (
    id             UUID          PRIMARY KEY DEFAULT uuid_generate_v4(),
    program_id     UUID          NOT NULL REFERENCES loyalty_programs(id) ON DELETE CASCADE,
    name           TEXT          NOT NULL,
    reward_type    TEXT          NOT NULL
                   CHECK (reward_type IN ('discount_fixed','discount_percent','free_shipping','gift_product','early_access')),
    points_cost    INT           NOT NULL CHECK (points_cost > 0),
    discount_value NUMERIC(10,2) CHECK (discount_value > 0),   -- montant ou pourcentage
    stock          INT           CHECK (stock >= 0),            -- NULL = illimité
    metadata       JSONB         NOT NULL DEFAULT '{}',         -- ex. {"variant_id": "..."}
    is_active      BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at     TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

-- ============================================================
-- RÉDEMPTIONS (utilisations de récompenses)
-- ============================================================

-- status : 'pending' | 'applied' | 'cancelled' | 'expired'
CREATE TABLE loyalty_redemptions (
    id           UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_id   UUID        NOT NULL REFERENCES loyalty_accounts(id),
    reward_id    UUID        NOT NULL REFERENCES loyalty_rewards(id),
    order_id     UUID        REFERENCES orders(id) ON DELETE SET NULL,
    points_spent INT         NOT NULL CHECK (points_spent > 0),
    status       TEXT        NOT NULL DEFAULT 'pending'
                 CHECK (status IN ('pending','applied','cancelled','expired')),
    redeemed_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_loyalty_redemptions_account ON loyalty_redemptions(account_id);

-- ============================================================
-- FONCTIONS MÉTIER
-- ============================================================

-- 1. Créditer des points (earn ou bonus) sur un compte
--    Recalcule le solde, les points lifetime, et met à jour le tier si besoin.
CREATE OR REPLACE FUNCTION loyalty_earn_points(
    p_account_id  UUID,
    p_points      INT,
    p_type        TEXT        DEFAULT 'earn',   -- 'earn' | 'bonus'
    p_order_id    UUID        DEFAULT NULL,
    p_description TEXT        DEFAULT NULL,
    p_expires_at  TIMESTAMPTZ DEFAULT NULL
)
RETURNS INT   -- nouveau solde
LANGUAGE plpgsql AS $$
DECLARE
    v_program_id    UUID;
    v_new_balance   INT;
    v_new_lifetime  INT;
    v_tier_id       UUID;
    v_expiry        TIMESTAMPTZ;
BEGIN
    IF p_points <= 0 THEN
        RAISE EXCEPTION 'points must be positive, got %', p_points;
    END IF;

    SELECT program_id INTO v_program_id
    FROM loyalty_accounts WHERE id = p_account_id;

    -- Calculer la date d'expiration si non fournie
    IF p_expires_at IS NULL THEN
        SELECT
            CASE WHEN expiry_days IS NOT NULL
                 THEN NOW() + (expiry_days || ' days')::INTERVAL
            END
        INTO v_expiry
        FROM loyalty_programs WHERE id = v_program_id;
    ELSE
        v_expiry := p_expires_at;
    END IF;

    -- Insérer la transaction
    INSERT INTO loyalty_transactions
        (account_id, order_id, type, points, description, expires_at)
    VALUES
        (p_account_id, p_order_id, p_type, p_points, p_description, v_expiry);

    -- Mettre à jour le solde et les points à vie
    UPDATE loyalty_accounts
    SET points_balance  = points_balance  + p_points,
        points_lifetime = points_lifetime + p_points
    WHERE id = p_account_id
    RETURNING points_balance, points_lifetime
    INTO v_new_balance, v_new_lifetime;

    -- Recalculer le tier (tier le plus élevé dont le seuil est atteint)
    SELECT id INTO v_tier_id
    FROM loyalty_tiers
    WHERE program_id = v_program_id
      AND min_points <= v_new_lifetime
    ORDER BY min_points DESC
    LIMIT 1;

    IF v_tier_id IS DISTINCT FROM (
        SELECT current_tier_id FROM loyalty_accounts WHERE id = p_account_id
    ) THEN
        UPDATE loyalty_accounts
        SET current_tier_id = v_tier_id,
            tier_updated_at = NOW()
        WHERE id = p_account_id;
    END IF;

    RETURN v_new_balance;
END;
$$;


-- 2. Calculer les points gagnés sur une commande (applique le multiplicateur de tier)
CREATE OR REPLACE FUNCTION loyalty_points_for_order(
    p_account_id UUID,
    p_order_id   UUID
)
RETURNS INT
LANGUAGE plpgsql AS $$
DECLARE
    v_total          NUMERIC;
    v_points_per_eur NUMERIC;
    v_multiplier     NUMERIC;
    v_points         INT;
BEGIN
    SELECT total_amount INTO v_total FROM orders WHERE id = p_order_id;

    SELECT lp.points_per_euro, COALESCE(lt.bonus_multiplier, 1)
    INTO v_points_per_eur, v_multiplier
    FROM loyalty_accounts la
    JOIN loyalty_programs lp ON lp.id = la.program_id
    LEFT JOIN loyalty_tiers lt ON lt.id = la.current_tier_id
    WHERE la.id = p_account_id;

    v_points := FLOOR(v_total * v_points_per_eur * v_multiplier)::INT;
    RETURN GREATEST(v_points, 0);
END;
$$;


-- 3. Utiliser une récompense (débiter des points et créer la rédemption)
CREATE OR REPLACE FUNCTION loyalty_redeem_reward(
    p_account_id UUID,
    p_reward_id  UUID,
    p_order_id   UUID DEFAULT NULL
)
RETURNS UUID   -- id de la rédemption
LANGUAGE plpgsql AS $$
DECLARE
    v_cost        INT;
    v_balance     INT;
    v_stock       INT;
    v_redemp_id   UUID;
BEGIN
    SELECT points_cost, stock INTO v_cost, v_stock
    FROM loyalty_rewards WHERE id = p_reward_id AND is_active = TRUE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'reward % not found or inactive', p_reward_id;
    END IF;

    SELECT points_balance INTO v_balance
    FROM loyalty_accounts WHERE id = p_account_id;

    IF v_balance < v_cost THEN
        RAISE EXCEPTION 'insufficient points: balance=%, cost=%', v_balance, v_cost;
    END IF;

    IF v_stock IS NOT NULL AND v_stock <= 0 THEN
        RAISE EXCEPTION 'reward % out of stock', p_reward_id;
    END IF;

    -- Décrémenter le stock si limité
    UPDATE loyalty_rewards
    SET stock = stock - 1
    WHERE id = p_reward_id AND stock IS NOT NULL;

    -- Déduire les points du solde
    UPDATE loyalty_accounts
    SET points_balance = points_balance - v_cost
    WHERE id = p_account_id;

    -- Enregistrer la transaction de débit
    INSERT INTO loyalty_transactions
        (account_id, order_id, reward_id, type, points, description)
    VALUES
        (p_account_id, p_order_id, p_reward_id, 'redeem', -v_cost,
         'Rédemption récompense');

    -- Créer la rédemption
    INSERT INTO loyalty_redemptions
        (account_id, reward_id, order_id, points_spent, status)
    VALUES
        (p_account_id, p_reward_id, p_order_id, v_cost, 'applied')
    RETURNING id INTO v_redemp_id;

    RETURN v_redemp_id;
END;
$$;


-- 4. Job d'expiration des points (à appeler via pg_cron ou cron externe, ex. chaque nuit)
CREATE OR REPLACE FUNCTION loyalty_expire_points()
RETURNS INT   -- nombre de comptes impactés
LANGUAGE plpgsql AS $$
DECLARE
    v_rec    RECORD;
    v_count  INT := 0;
    v_expire INT;
BEGIN
    FOR v_rec IN
        SELECT account_id, SUM(points) AS pts_to_expire
        FROM loyalty_transactions
        WHERE type IN ('earn','bonus')
          AND expires_at <= NOW()
          AND expires_at IS NOT NULL
          AND NOT EXISTS (
              SELECT 1 FROM loyalty_transactions t2
              WHERE t2.account_id    = loyalty_transactions.account_id
                AND t2.type         = 'expire'
                AND t2.created_at::DATE = loyalty_transactions.expires_at::DATE
          )
        GROUP BY account_id
    LOOP
        v_expire := LEAST(v_rec.pts_to_expire,
                          (SELECT points_balance FROM loyalty_accounts WHERE id = v_rec.account_id));

        IF v_expire > 0 THEN
            INSERT INTO loyalty_transactions
                (account_id, type, points, description)
            VALUES
                (v_rec.account_id, 'expire', -v_expire, 'Expiration automatique des points');

            UPDATE loyalty_accounts
            SET points_balance = points_balance - v_expire
            WHERE id = v_rec.account_id;

            v_count := v_count + 1;
        END IF;
    END LOOP;

    RETURN v_count;
END;
$$;


-- ============================================================
-- DONNÉES DE RÉFÉRENCE
-- ============================================================

DO $$
DECLARE
    v_prog UUID;
    v_bronze UUID; v_silver UUID; v_gold UUID; v_plat UUID;
BEGIN
    INSERT INTO loyalty_programs (name, points_per_euro, euro_per_point, expiry_days)
    VALUES ('Programme Fidélité', 10, 0.01, 365)
    RETURNING id INTO v_prog;

    INSERT INTO loyalty_tiers (program_id, name, min_points, bonus_multiplier, rank, perks) VALUES
        (v_prog, 'Bronze',   0,     1.0, 1, '{}'),
        (v_prog, 'Silver',   5000,  1.5, 2, '{"free_shipping_threshold": 30}'),
        (v_prog, 'Gold',     20000, 2.0, 3, '{"free_shipping_threshold": 0, "early_access": true}'),
        (v_prog, 'Platinum', 60000, 3.0, 4, '{"free_shipping_threshold": 0, "early_access": true, "dedicated_support": true}')
    RETURNING id INTO v_bronze;

    INSERT INTO loyalty_rewards
        (program_id, name, reward_type, points_cost, discount_value) VALUES
        (v_prog, 'Bon de réduction 5 €',   'discount_fixed',   500,  5.00),
        (v_prog, 'Bon de réduction 15 €',  'discount_fixed',   1400, 15.00),
        (v_prog, 'Réduction 10 %',         'discount_percent', 800,  10.00),
        (v_prog, 'Livraison offerte',       'free_shipping',    300,  NULL),
        (v_prog, 'Accès vente privée',      'early_access',     1000, NULL);
END;
$$;

-- ============================================================
-- VUE : tableau de bord fidélité par utilisateur
-- ============================================================
CREATE VIEW v_loyalty_dashboard AS
SELECT
    u.id            AS user_id,
    u.full_name,
    u.email,
    la.points_balance,
    la.points_lifetime,
    lt.name         AS tier_name,
    lt.bonus_multiplier,
    lt.perks,
    -- Points nécessaires pour atteindre le tier suivant
    (
        SELECT nt.min_points - la.points_lifetime
        FROM loyalty_tiers nt
        WHERE nt.program_id = la.program_id
          AND nt.rank > lt.rank
        ORDER BY nt.rank ASC
        LIMIT 1
    )               AS points_to_next_tier,
    la.tier_updated_at,
    la.created_at   AS member_since
FROM loyalty_accounts la
JOIN users u          ON u.id  = la.user_id
LEFT JOIN loyalty_tiers lt ON lt.id = la.current_tier_id;
