# GestionSalleEmploiTemps

## 1. Présentation du projet
**GestionSalleEmploiTemps** est une application web développée en ASP.NET Core MVC. 
Son objectif principal est d'optimiser et de centraliser la gestion des salles et des emplois du temps universitaires. L'application permet à l'administration et aux enseignants de planifier, consulter et gérer les séances de cours sans risque de chevauchement de salles ou d'enseignants.

## 2. Fonctionnalités principales
* **Authentification des utilisateurs** : Système sécurisé avec ASP.NET Identity (Inscription, Connexion, Déconnexion).
* **Gestion des salles** : Suivi des disponibilités des salles de l'établissement.
* **Gestion des cours** : Planification des matières enseignées et prévention des conflits horaires.
* **Gestion des emplois du temps** : Affichage interactif des plannings hebdomadaires.
* **Gestion du profil utilisateur** : Modification sécurisée de ses propres données par chaque enseignant.
* **Informations personnelles et professionnelles** : Séparation claire des données (Diplômes, spécialisations, informations de contact).
* **Pagination** : Navigation fluide dans les longues listes de données grâce à une pagination côté serveur optimisée.
* **Générateur PDF des emplois du temps** : Exportation professionnelle et instantanée des plannings (A4 Paysage) via QuestPDF.

## 3. Technologies utilisées
* **ASP.NET Core MVC** (.NET 8)
* **Entity Framework Core** (ORM)
* **SQL Server** (Base de données relationnelle)
* **ASP.NET Identity** (Gestion de la sécurité et de l'authentification)
* **Bootstrap 5** (Design moderne et responsive)
* **QuestPDF** (Bibliothèque de génération PDF avancée)

## 4. Architecture du projet
Le projet suit scrupuleusement l'architecture **MVC (Modèle-Vue-Contrôleur)** :
* **Models** : Définit la structure des données et la logique métier de l'application (ex: `Cours`, `Professeur`, `Salle`).
* **Views** : Les fichiers Razor (`.cshtml`) responsables de l'interface graphique dynamique envoyée au navigateur.
* **Controllers** : Réceptionnent les requêtes HTTP de l'utilisateur, traitent la logique avec les Models, et retournent les Views appropriées.
* **Services** : Couche d'abstraction métier (comme `PdfGeneratorService`) permettant d'alléger les contrôleurs et de respecter le principe de responsabilité unique.
* **Data** : Contient `ApplicationDbContext`, le pont principal entre l'application C# et la base de données SQL Server.
* **Entity Framework Core** : L'outil utilisé pour traduire nos objets C# directement en tables SQL et requêtes optimisées.

## 5. Configuration SQL Server
Pour que l'application puisse communiquer avec la base de données, vous devez configurer la chaîne de connexion.

1. Modifiez le fichier `appsettings.json` à la racine de votre projet avec votre chaîne de connexion SQL Server :
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=GestionSalleEmploiTemps;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
2. Ouvrez la Console du Gestionnaire de packages (ou votre terminal).
3. Appliquez les migrations existantes pour générer la structure de la base de données :
```powershell
Update-Database
```
*(Si vous utilisez l'interface en ligne de commande .NET CLI, tapez : `dotnet ef database update`)*.

## 6. Installation du projet

Suivez ces étapes pour installer et exécuter le projet localement :

1. **Cloner le dépôt GitHub** :
   ```bash
   git clone https://github.com/votre-utilisateur/GestionSalleEmploiTemps.git
   cd GestionSalleEmploiTemps
   ```
2. **Installer les dépendances** (Nuget Packages) :
   ```bash
   dotnet restore
   ```
3. **Configurer SQL Server** : 
   - Vérifiez et modifiez la chaîne de connexion dans `appsettings.json`.
   - Lancez la création de la base de données (`Update-Database` ou `dotnet ef database update`).
4. **Lancer l'application** :
   ```bash
   dotnet run
   ```
   *L'application sera ensuite accessible depuis votre navigateur via l'adresse indiquée dans le terminal (ex: `http://localhost:5xxx`).*

## 7. Structure du projet

Voici une vue simplifiée de l'arborescence du projet :

```text
GestionSalleEmploiTemps/
├── Controllers/      # Logique de navigation
├── Data/             # Contexte de base de données (ApplicationDbContext)
├── Models/           # Entités et ViewModels
├── Services/         # Logique métier spécifique (PDF, etc.)
├── Views/            # Interfaces utilisateur Razor
└── wwwroot/          # Fichiers statiques (CSS, JS, Images)
```

## 8. Auteur
Application développée par **Ntsoa** (ou le nom de votre équipe) dans le cadre de la gestion universitaire de l'EMIT.