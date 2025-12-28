# Robot Trading Application v3

## ⚠️ AVERTISSEMENT - PROJET EN LECTURE SEULE

**Ce code est strictement protégé par une licence propriétaire. Toute copie, modification, distribution ou réutilisation de ce code est strictement interdite. Voir le fichier [LICENSE.md](LICENSE.md) pour plus de détails.**

## 📋 À propos du projet

Ce projet est un **laboratoire personnel** développé dans le cadre de mon apprentissage et de mon perfectionnement en développement logiciel. Il représente une exploration de mes deux passions principales :

- 🎯 **Le trading algorithmique** - Une passion de longue date qui m'a motivé à créer des outils d'analyse et d'automatisation
- 💻 **Le développement logiciel** - Notamment l'apprentissage approfondi du C# (.NET) et de React/TypeScript

### Objectifs d'apprentissage

Ce projet m'a permis de développer et consolider mes compétences sur :

- **Backend** : C# / .NET, ASP.NET Core, Entity Framework Core
- **Frontend** : React, TypeScript, Vite, TailwindCSS
- **Architecture** : Clean Architecture, Repository Pattern, Dependency Injection
- **Communication temps réel** : SignalR pour les WebSockets
- **Compilation dynamique** : Chargement et compilation de stratégies à la volée
- **Containerisation** : Docker et Docker Compose
- **Tests** : Tests unitaires avec xUnit
- **Envoi d'emails** : Service d'envoi de mails
- **Base de données** : Gestion de contexte et repositories

## 🔍 Fonctionnement général

Cette application est une **plateforme de trading algorithmique** permettant de développer, tester et exécuter des stratégies de trading automatisées sur les marchés financiers.

### Principe de base

1. **Connexion aux marchés** : L'application se connecte à des APIs de courtiers/plateformes de trading pour récupérer les données de marché en temps réel (prix, chandeliers, ticks)

2. **Analyse en temps réel** : Les données sont analysées à l'aide d'indicateurs techniques (moyennes mobiles, RSI, MACD, etc.) et de patterns de chandeliers

3. **Exécution de stratégies** : Des stratégies de trading (écrites en C#) sont compilées dynamiquement et exécutées pour générer des signaux d'achat/vente

4. **Gestion des positions** : Le système gère automatiquement l'ouverture et la fermeture de positions selon les signaux générés

5. **Monitoring et notifications** : L'interface React affiche en temps réel l'état du système, les positions ouvertes, les performances, et envoie des notifications par email lors d'événements importants

### Flux de données

```
API Broker → Connecteurs → RobotAppLibrary → Stratégies → Signaux
                                ↓                           ↓
                          Base de données ← Services ← Décisions
                                ↓                           ↓
                          SignalR Hubs → Frontend React
```

## 🏗️ Architecture du projet

### Projets Backend (.NET)

#### Couche API et Serveur
- **robot-project-v3.Server** : Point d'entrée principal de l'application
  - Controllers REST pour les opérations CRUD
  - Hubs SignalR pour la communication temps réel avec le frontend
  - Background Services pour les tâches périodiques et l'exécution des stratégies
  - Gestion centralisée des exceptions avec enrichissement des logs (Serilog)
  - Mappers pour la conversion entre entités de base de données et DTOs

#### Couche Données
- **robot-project-v3.Database** : Persistance des données
  - DbContext Entity Framework Core pour la gestion de la base de données
  - Pattern Repository pour l'abstraction de l'accès aux données
  - Modèles d'entités représentant les tables (Positions, Transactions, Configurations, etc.)
  - Migrations pour le versioning du schéma de base de données

#### Services Transverses
- **robot-project-v3.Mail** : Service d'envoi d'emails
  - Configuration SMTP
  - Notifications automatiques sur événements (positions ouvertes/fermées, erreurs, alertes)

#### Bibliothèques Trading
- **RobotAppLibrary** : Cœur métier de l'application de trading
  - **Indicators/** : Indicateurs techniques (SMA, EMA, RSI, MACD, Bollinger Bands, etc.)
  - **Chart/** : Analyse des patterns de chandeliers (Doji, Hammer, Engulfing, etc.)
  - **Strategy/** : Classes de base pour créer des stratégies de trading
  - **TradingManager/** : Gestion du cycle de vie des positions et du risk management
  - **Factory/** : Factories pour l'instanciation des composants
  - **LLM/** : Intégration expérimentale avec des modèles de langage pour l'analyse

- **RobotAppLibrary.Api** : Abstraction des APIs de trading
  - **Connector/** : Connecteurs pour différents brokers/plateformes
  - **Executor/** : Exécution des ordres de marché
  - **Providers/** : Fournisseurs de données de marché (prix, chandeliers, ticks)
  - **Interfaces/** : Contrats d'interface pour l'interopérabilité

- **RobotAppLibrary.Modeles** : Modèles de domaine partagés
  - `Candle` : Chandeliers OHLC (Open, High, Low, Close)
  - `Tick` : Données tick-by-tick
  - `Position` : Positions de trading ouvertes/fermées
  - `Signal` : Signaux d'achat/vente générés par les stratégies
  - `AccountBalance` : Solde et équité du compte
  - `SymbolInfo` : Informations sur les instruments tradés
  - Attributs personnalisés pour la sérialisation et validation

- **RobotAppLibrary.StrategyDynamicCompiler** : Compilation à la volée
  - Compilation dynamique de code C# en assemblies
  - `CustomLoadContext` : Contexte de chargement isolé pour les stratégies
  - Gestion des dépendances et références
  - Permet de modifier et recharger des stratégies sans redémarrer l'application

- **RobotAppLibrary.LLM** : Intégration IA
  - Repositories pour stocker les analyses LLM
  - Expérimentation avec l'analyse de sentiment et la prédiction assistée par IA

### Projet Frontend

- **robot-project-v3.client** : Interface utilisateur moderne et réactive
  - **React 18** avec hooks pour la gestion d'état
  - **TypeScript** pour la sécurité de type
  - **Vite** pour un build ultra-rapide et HMR (Hot Module Replacement)
  - **TailwindCSS** pour le styling utilitaire
  - **SignalR Client** pour recevoir les mises à jour temps réel depuis le serveur
  - Tableaux de bord affichant :
    - Positions ouvertes et historique
    - Graphiques de performance
    - Logs et événements en temps réel
    - Configuration des stratégies
    - Statistiques de trading

### Utilitaires

- **DtoTsGenerator** : Pont entre Backend et Frontend
  - Génère automatiquement des interfaces TypeScript à partir des classes C# annotées avec `[Dto]`
  - Garantit la cohérence des types entre le backend .NET et le frontend React
  - Évite les erreurs de typage lors de la communication API
  - S'exécute au build pour synchroniser les modèles

## 🛠️ Stack technique

### Backend
- .NET / C#
- ASP.NET Core
- Entity Framework Core
- SignalR
- Serilog (logging enrichi)

### Frontend
- React
- TypeScript
- Vite
- TailwindCSS

### Infrastructure
- Docker & Docker Compose
- Base de données (via EF Core)

## 📊 Fonctionnalités détaillées

### 📈 Analyse de marché
- **Données en temps réel** : Réception de ticks et chandeliers en direct depuis les APIs de brokers
- **Indicateurs techniques** : 
  - Moyennes mobiles (SMA, EMA, WMA)
  - Oscillateurs (RSI, Stochastic, MACD)
  - Bandes de Bollinger
  - ATR (Average True Range) pour la volatilité
  - Volume et OBV (On Balance Volume)
- **Patterns de chandeliers** : Détection automatique de patterns (Doji, Hammer, Shooting Star, Engulfing, etc.)
- **Multi-timeframes** : Analyse simultanée sur différentes périodes (M1, M5, M15, H1, H4, D1)

### 🤖 Stratégies de trading
- **Développement personnalisé** : Création de stratégies en C# héritant d'une classe de base
- **Compilation dynamique** : 
  - Chargement de fichiers .cs à la volée
  - Compilation en mémoire sans redémarrage
  - Isolation via `AssemblyLoadContext` pour éviter les conflits
- **Backtesting** : Test des stratégies sur données historiques
- **Signaux automatiques** : Génération de signaux BUY/SELL basés sur la logique de la stratégie

### 💼 Gestion de trading
- **Ordres automatisés** : Ouverture et fermeture de positions selon les signaux
- **Risk Management** :
  - Stop Loss et Take Profit automatiques
  - Taille de position calculée selon le risque
  - Trailing Stop pour sécuriser les gains
- **Suivi des positions** : 
  - Positions ouvertes avec P&L en temps réel
  - Historique complet des transactions
  - Statistiques de performance (win rate, profit factor, drawdown)

### 🔔 Notifications et monitoring
- **WebSockets (SignalR)** : 
  - Mise à jour instantanée de l'interface
  - Push des nouveaux signaux, positions, et événements
  - Synchronisation multi-clients
- **Emails automatiques** : Notifications configurables sur événements critiques
- **Logs enrichis** : Traçabilité complète avec Serilog et enrichisseurs personnalisés

### 🎛️ Interface utilisateur
- **Dashboard temps réel** : Vue d'ensemble des positions et performances
- **Configuration** : Paramétrage des stratégies via l'interface
- **Graphiques** : Visualisation des courbes de performance
- **Mode sombre/clair** : Interface adaptative avec TailwindCSS

## 📖 Statut du projet

**⏸️ PROJET À L'ARRÊT**

Ce projet est actuellement **inactif** et **archivé**. Ayant atteint mes objectifs d'apprentissage initiaux, je me suis orienté vers d'autres projets et technologies. Le code est partagé ici uniquement à des **fins de démonstration** et reste ma propriété exclusive.

**Aucun support, maintenance ou mise à jour ne sera fourni.**

## ⚖️ Licence

Ce projet est protégé par une **licence propriétaire stricte**. 

**DROITS D'AUTEUR - TOUS DROITS RÉSERVÉS**

- ❌ Aucune copie autorisée
- ❌ Aucune modification autorisée
- ❌ Aucune distribution autorisée
- ❌ Aucune utilisation commerciale autorisée
- ❌ Aucune réutilisation partielle ou totale autorisée
- ✅ Consultation et lecture uniquement à des fins de référence personnelle

Voir [LICENSE.md](LICENSE.md) pour les termes complets.

---

© 2024-2025 - Tous droits réservés. Ce code est la propriété exclusive de son auteur.
