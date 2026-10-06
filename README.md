# Ecommerce

Plateforme e-commerce en .NET : catalogue, panier, commandes, paiements et programme de fidélité.

## Architecture

Monolithe modulaire. Chaque module porte ses cas d'usage en CQRS avec MediatR et Wolverine :

- `src/Api/` : point d'entrée HTTP. Contrôleurs de commande (`CheckoutController`) et de webhooks
  (`WebhooksController`), plus deux services d'arrière-plan (`LoyaltyExpiryBackgroundService`,
  `NotificationsBackgroundService`)
- `src/Modules/` : modules métier, dont panier et commandes, chacun avec ses commandes et ses
  gestionnaires
- `src/Infrastructure/` : persistance, messagerie et intégrations externes. Configuration Wolverine
  dans `WolverineConfiguration.cs`, intégration Stripe dans `StripeConnectService` et
  `StripePaymentService`
- `src/Shared/` : types communs
- `database/` : scripts SQL (schéma, fidélité, extensions)
- `tests/` : tests d'intégration de l'API et tests du module fidélité

## Stack

.NET 8, ASP.NET Core, MediatR, Wolverine, Entity Framework Core, PostgreSQL, Redis, Stripe,
Docker Compose, tests d'intégration.

## Lancer le projet

```bash
docker-compose up --build
```

L'API écoute sur le port 5000, PostgreSQL sur 5432 et Redis sur son port habituel. Les chaînes de
connexion et les clés Stripe se passent par variables d'environnement.

## Tests

```bash
dotnet test
```

## État

Checkout, webhooks Stripe, fidélité et notifications en place, avec tests d'intégration. Les
résultats de tests stockés dans `tests/**/TestResults/` sont des sorties d'exécution : elles n'ont
pas leur place dans le dépôt.
