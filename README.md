
<div align="center">
  <a href="https://dotnet.microsoft.com/" target="_blank">
    <img src="https://upload.wikimedia.org/wikipedia/commons/e/ee/.NET_Core_Logo.svg" width="120" alt=".NET Logo" />
  </a>
</div>
<div align="center">
  <a href="https://dotnet.microsoft.com/" target="_blank">
    <img src="https://img.shields.io/badge/built%20with-.NET%207-blue" alt=".NET 7" />
  </a>
  <a href="https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-7.0" target="_blank">
    <img src="https://img.shields.io/badge/ASP.NET-Core-512BD4?logo=.net" alt="ASP.NET Core">
  </a>
  <a href="https://www.microsoft.com/sql-server/" target="_blank">
    <img src="https://img.shields.io/badge/SQL-Server-orange" alt="SQL Server">
  </a>
</div>

---

### English Version
# Eulalia Backend – Digital voter ID

**STATUS**: ACTIVE

### Table of Contents
1. [Team](#team)
2. [Project Objective](#project-objective)
3. [Target Audience](#target-audience)
4. [Architecture](#architecture)
5. [Technologies](#technologies)
6. [Installation](#installation)
7. [Environment Setup](#environment-setup)
8. [Running](#running)
9. [Notes](#notes)

## Team
* **David Tacuri**

## Project Objective
The backend for the Eulalia system, a decentralized digital political affiliation platform that uses blockchain to empower citizens to securely manage their affiliation to political parties through self-sovereign digital identity (DID).

## Target Audience
Citizens, political organizations, electoral authorities, and civic technology innovators interested in transparency, participation, and digital sovereignty.

## Architecture

| Layer        | Description                                 |
|--------------|---------------------------------------------|
| Api          | REST controllers and startup                |
| Application  | Business logic and use cases                |
| Domain       | Core entities and business rules            |
| Infrastructure | External services, DB access, auth, etc.  |
| Shared       | Common helpers, DTOs, interfaces, etc.      |

## Technologies

- ASP.NET Core 7
- C#
- Clean Architecture
- PostgreSQL
- JWT Authentication
- DID integration potential (e.g. Atala PRISM)
- Entity Framework Core

## Installation

```bash
git clone https://github.com/democracyonchain/eulalia-backend.git
```

Open the solution `eulalia-backend.sln` with Visual Studio 2022.

## Environment Setup

You must create your own `appsettings.json` file inside `eulalia-backend.Api/`.

Refer to `appsettings.Development.json` as a template.

> ⚠️ `appsettings.json` is excluded from the repo to protect credentials and secrets.

## Running

From Visual Studio:

- Set `eulalia-backend.Api` as startup project
- Press `F5` to run in Development mode

Or from terminal:

```bash
cd eulalia-backend.Api
dotnet run
```

---

### Spanish Version
# Backend de Eulalia – Sistema Descentralizado de Afiliación Política Digital

**ESTADO**: ACTIVO

### Tabla de Contenidos
1. [Equipo](#equipo)
2. [Objetivo del Proyecto](#objetivo-del-proyecto)
3. [Dirigido a](#dirigido-a)
4. [Arquitectura](#arquitectura)
5. [Tecnologías](#tecnologías)
6. [Instalación](#instalación)
7. [Configuración de Entorno](#configuración-de-entorno)
8. [Ejecución](#ejecución)
9. [Notas](#notas)

## Equipo
* **David Tacuri**

## Objetivo del Proyecto
Backend del sistema Eulalia, una plataforma digital de afiliación política que permite a los ciudadanos autogestionar su afiliación a partidos mediante identidad digital descentralizada (DID) y blockchain, garantizando seguridad, trazabilidad y transparencia.

## Dirigido a
Ciudadanos, partidos políticos, autoridades electorales y desarrolladores interesados en soberanía digital y participación democrática.

## Arquitectura

| Capa           | Descripción                                |
|----------------|---------------------------------------------|
| Api            | Controladores REST y configuración inicial |
| Application    | Lógica de negocio                          |
| Domain         | Entidades principales y reglas de negocio  |
| Infrastructure | Acceso a datos, servicios externos         |
| Shared         | Utilidades, interfaces y DTOs compartidos  |

## Tecnologías

- ASP.NET Core 7
- C#
- Arquitectura Limpia
- PostgreSQL
- Autenticación con JWT
- Integración potencial con DID (por ejemplo, Atala PRISM)
- Entity Framework Core

## Instalación

```bash
git clone https://github.com/democracyonchain/eulalia-backend.git
```

Abre `eulalia-backend.sln` con Visual Studio 2022.

## Configuración de Entorno

Debes crear un archivo `appsettings.json` en `eulalia-backend.Api/`.

Usa como base el `appsettings.Development.json`.

> ⚠️ El archivo `appsettings.json` está excluido por seguridad.

## Ejecución

Desde Visual Studio:

- Establece `eulalia-backend.Api` como proyecto de inicio
- Presiona `F5`

Desde consola:

```bash
cd eulalia-backend.Api
dotnet run
```


## Configuration Example (appsettings.json)

> This file must be created inside `eulalia-backend.Api/` and is excluded from version control.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=;Username=;Password!"
  },
  "Jwt": {
    "Key": "clave-secreta-para-jwt-de-mas-de-32-caracteres-1234",
    "Issuer": "eulalia.frontend",
    "Audience": "eulalia.frontend",
    "ExpirationMinutes": 60
  },
  "AllowedHosts": "*"
}
```

---

## Ejemplo de configuración (`appsettings.json`)

> Este archivo debe ser creado dentro de `eulalia-backend.Api/` y está excluido del control de versiones.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=;Username=;Password!"
  },
  "Jwt": {
    "Key": "clave-secreta-para-jwt-de-mas-de-32-caracteres-1234",
    "Issuer": "eulalia.frontend",
    "Audience": "eulalia.frontend",
    "ExpirationMinutes": 60
  },
  "AllowedHosts": "*"
}
```


## Notas

- Este proyecto es parte de las soluciones cívicas impulsadas por DemocracyOnChain, enfocadas en identidad, participación y confianza digital.
