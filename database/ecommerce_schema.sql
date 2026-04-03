-- ============================================================
-- Schéma de base de données — E-commerce / Marketplace
-- Dialecte : PostgreSQL
-- ============================================================

-- Extensions utiles
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "citext";      -- emails insensibles à la casse

-- ------------------------------------------------------------
-- USERS — acheteurs et vendeurs partagent la même table
-- ------------------------------------------------------------
CREATE TABLE users (
    id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email        CITEXT      NOT NULL UNIQUE,
    full_name    TEXT        NOT NULL,
    phone        TEXT,
    -- 'buyer' | 'seller' | 'admin'
    role         TEXT        NOT NULL DEFAULT 'buyer' CHECK (role IN ('buyer','seller','admin')),
    password_hash TEXT       NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ------------------------------------------------------------
-- ADDRESSES — adresses de livraison liées à un utilisateur
-- ------------------------------------------------------------
CREATE TABLE addresses (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    street      TEXT        NOT NULL,
    city        TEXT        NOT NULL,
    postal_code TEXT        NOT NULL,
    country     TEXT        NOT NULL DEFAULT 'FR',
    is_default  BOOLEAN     NOT NULL DEFAULT FALSE,
    CONSTRAINT one_default_per_user UNIQUE (user_id, is_default)  -- partiel, voir note ci-dessous
);
-- Note : pour n'autoriser qu'une seule adresse par défaut par utilisateur,
-- préférez un index partiel :
-- CREATE UNIQUE INDEX uq_default_address ON addresses(user_id) WHERE is_default = TRUE;

-- ------------------------------------------------------------
-- SELLERS — profil vendeur, extension optionnelle d'un user
-- ------------------------------------------------------------
CREATE TABLE sellers (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id     UUID        NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    shop_name   TEXT        NOT NULL,
    description TEXT,
    rating      NUMERIC(3,2) CHECK (rating BETWEEN 0 AND 5),
    is_verified BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ------------------------------------------------------------
-- CATEGORIES — arborescence avec auto-référence (parent_id)
-- ------------------------------------------------------------
CREATE TABLE categories (
    id        UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    parent_id UUID REFERENCES categories(id) ON DELETE SET NULL,
    name      TEXT NOT NULL,
    slug      TEXT NOT NULL UNIQUE
);

-- ------------------------------------------------------------
-- PRODUCTS — catalogue produit d'un vendeur
-- ------------------------------------------------------------
CREATE TABLE products (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    seller_id   UUID        NOT NULL REFERENCES sellers(id) ON DELETE CASCADE,
    category_id UUID        REFERENCES categories(id) ON DELETE SET NULL,
    name        TEXT        NOT NULL,
    description TEXT,
    base_price  NUMERIC(12,2) NOT NULL CHECK (base_price >= 0),
    is_active   BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ------------------------------------------------------------
-- PRODUCT_VARIANTS — déclinaisons (taille, couleur, stock…)
-- ------------------------------------------------------------
CREATE TABLE product_variants (
    id             UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id     UUID        NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    sku            TEXT        NOT NULL UNIQUE,
    color          TEXT,
    size           TEXT,
    -- NULL = hérite de base_price du produit
    price          NUMERIC(12,2) CHECK (price >= 0),
    stock_quantity INT         NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0)
);

-- ------------------------------------------------------------
-- CARTS — panier persistant (un seul par utilisateur)
-- ------------------------------------------------------------
CREATE TABLE carts (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id    UUID        NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE cart_items (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cart_id    UUID NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    variant_id UUID NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    quantity   INT  NOT NULL DEFAULT 1 CHECK (quantity > 0),
    UNIQUE (cart_id, variant_id)
);

-- ------------------------------------------------------------
-- ORDERS — commandes passées
-- ------------------------------------------------------------
CREATE TABLE orders (
    id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id      UUID        NOT NULL REFERENCES users(id),
    address_id   UUID        NOT NULL REFERENCES addresses(id),
    -- 'pending' | 'confirmed' | 'shipped' | 'delivered' | 'cancelled'
    status       TEXT        NOT NULL DEFAULT 'pending'
                 CHECK (status IN ('pending','confirmed','shipped','delivered','cancelled')),
    total_amount NUMERIC(12,2) NOT NULL CHECK (total_amount >= 0),
    ordered_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ------------------------------------------------------------
-- ORDER_ITEMS — lignes d'une commande (snapshot du prix)
-- ------------------------------------------------------------
CREATE TABLE order_items (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id   UUID          NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    variant_id UUID          NOT NULL REFERENCES product_variants(id),
    quantity   INT           NOT NULL CHECK (quantity > 0),
    -- Prix unitaire figé au moment de la commande
    unit_price NUMERIC(12,2) NOT NULL CHECK (unit_price >= 0)
);

-- ------------------------------------------------------------
-- PAYMENTS — paiement associé à une commande (1-1)
-- ------------------------------------------------------------
CREATE TABLE payments (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id        UUID        NOT NULL UNIQUE REFERENCES orders(id) ON DELETE CASCADE,
    -- 'card' | 'paypal' | 'bank_transfer' | 'crypto'
    method          TEXT        NOT NULL,
    -- 'pending' | 'completed' | 'failed' | 'refunded'
    status          TEXT        NOT NULL DEFAULT 'pending'
                    CHECK (status IN ('pending','completed','failed','refunded')),
    amount          NUMERIC(12,2) NOT NULL CHECK (amount >= 0),
    transaction_ref TEXT        UNIQUE,
    paid_at         TIMESTAMPTZ
);

-- ------------------------------------------------------------
-- REVIEWS — avis produit rattachés à un achat vérifié
-- ------------------------------------------------------------
CREATE TABLE reviews (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    -- Vérification d'achat : l'avis doit venir d'une commande réelle
    order_id   UUID NOT NULL REFERENCES orders(id),
    rating     INT  NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment    TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    -- Un avis par utilisateur par produit par commande
    UNIQUE (user_id, product_id, order_id)
);

-- ============================================================
-- INDEX — performances courantes
-- ============================================================
CREATE INDEX idx_products_seller      ON products(seller_id);
CREATE INDEX idx_products_category    ON products(category_id);
CREATE INDEX idx_products_active      ON products(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_variants_product     ON product_variants(product_id);
CREATE INDEX idx_order_items_order    ON order_items(order_id);
CREATE INDEX idx_order_items_variant  ON order_items(variant_id);
CREATE INDEX idx_orders_user          ON orders(user_id);
CREATE INDEX idx_orders_status        ON orders(status);
CREATE INDEX idx_reviews_product      ON reviews(product_id);
CREATE INDEX idx_cart_items_cart      ON cart_items(cart_id);

-- ============================================================
-- FONCTIONS UTILITAIRES
-- ============================================================

-- Mise à jour automatique de updated_at
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_users_updated_at
  BEFORE UPDATE ON users
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_products_updated_at
  BEFORE UPDATE ON products
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_orders_updated_at
  BEFORE UPDATE ON orders
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_carts_updated_at
  BEFORE UPDATE ON carts
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ============================================================
-- VUES PRATIQUES
-- ============================================================

-- Résumé produit avec prix effectif de chaque variante
CREATE VIEW v_product_variants AS
SELECT
    p.id             AS product_id,
    p.name           AS product_name,
    s.shop_name,
    pv.id            AS variant_id,
    pv.sku,
    pv.color,
    pv.size,
    COALESCE(pv.price, p.base_price) AS effective_price,
    pv.stock_quantity
FROM products p
JOIN sellers s ON s.id = p.seller_id
JOIN product_variants pv ON pv.product_id = p.id
WHERE p.is_active = TRUE;

-- Statut complet d'une commande avec paiement
CREATE VIEW v_order_summary AS
SELECT
    o.id             AS order_id,
    u.email,
    u.full_name,
    o.status         AS order_status,
    o.total_amount,
    py.status        AS payment_status,
    py.method        AS payment_method,
    o.ordered_at
FROM orders o
JOIN users u  ON u.id = o.user_id
LEFT JOIN payments py ON py.order_id = o.id;
