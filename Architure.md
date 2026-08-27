# Arquitetura do Monólito Modular - FIAP Cloud Games

Este projeto segue a abordagem de monólito modular, mantendo um único deploy e uma base compartilhada, mas separando responsabilidades por domínio de negócio. A estrutura conceitual é organizada em módulos com os seguintes limites:

## 1. Visão de módulos

A organização modular do sistema é a seguinte:

```text
src/
│
├── FIAP.CloudGames.Api
│
└── Modules/
    │
    ├── Identity/
    │   │
    │   ├── Domain/
    │   │   └── Entities/
    │   │       ├── Usuario.cs
    │   │       ├── Perfil.cs
    │   │       ├── Token.cs
    │   │       ├── Autorizacao.cs
    │   │       └── Permissao.cs
    │   │
    │   ├── Application/
    │   │   ├── Abstractions/
    │   │   │   └── Repositories/
    │   │   │       └── IRepositorioUsuarios.cs
    │   │   ├── Usuarios/
    │   │   │   └── CriarUsuario/
    │   │   │       ├── ComandoCriarUsuario.cs
    │   │   │       ├── ManipuladorCriarUsuario.cs
    │   │   │       └── ResultadoCriarUsuario.cs
    │   │   └── IoC/
    │   │       └── IdentityDependency.cs
    │   │
    │   ├── Infrastructure/
    │   │   ├── Data/
    │   │   │   └── EF/
    │   │   │       ├── Context/
    │   │   │       │   └── IdentityDbContext.cs
    │   │   │       ├── Mappings/
    │   │   │       │   ├── MapeamentoUsuario.cs
    │   │   │       │   ├── MapeamentoPerfil.cs
    │   │   │       │   ├── MapeamentoTokens.cs
    │   │   │       │   ├── MapeamentoAutorizacao.cs
    │   │   │       │   └── MapeamentoPermissao.cs
    │   │   │       └── Migrations/
    │   │   ├── Repositories/
    │   │   │   └── RepositorioUsuarios.cs
    │   │   └── IoC/
    │   │       └── IdentityInfrastructureDependency.cs
    │   │
    │   └── Api/
    │       ├── Contracts/
    │       │   └── Usuarios/
    │       │       ├── RequisicaoCriarUsuario.cs
    │       │       └── RespostaCriarUsuario.cs
    │       └── Controllers/
    │           └── UsuariosController.cs
    │
    │
    ├── Catalog/
    │   │
    │   ├── Domain/
    │   │   └── Entities/
    │   │       ├── Jogo.cs
    │   │       ├── Categoria.cs
    │   │       └── CategoriaJogo.cs
    │   │
    │   ├── Application/
    │   │   ├── Abstractions/
    │   │   ├── Jogos/
    │   │   ├── Categorias/
    │   │   └── IoC/
    │   │       └── CatalogDependency.cs
    │   │
    │   ├── Infrastructure/
    │   │   ├── Data/
    │   │   │   └── EF/
    │   │   │       ├── Context/
    │   │   │       ├── Mappings/
    │   │   │       │   ├── MapeamentoJogo.cs
    │   │   │       │   ├── CategoriaMapping.cs
    │   │   │       │   └── MapeamentoCategoriaJogo.cs
    │   │   │       └── Migrations/
    │   │   ├── Repositories/
    │   │   └── IoC/
    │   │       └── CatalogInfrastructureDependency.cs
    │   │
    │   └── Api/
    │       ├── Contracts/
    │       │   ├── Jogos/
    │       │   └── Categorias/
    │       └── Controllers/
    │           ├── JogosController.cs
    │           └── CategoriasController.cs
    │
    │
    ├── Acquisition/
    │   │
    │   ├── Domain/
    │   │   └── Entities/
    │   │       └── Aquisicao.cs
    │   │
    │   ├── Application/
    │   │   ├── Abstractions/
    │   │   ├── Aquisicoes/
    │   │   └── IoC/
    │   │       └── AcquisitionDependency.cs
    │   │
    │   ├── Infrastructure/
    │   │   ├── Data/
    │   │   │   └── EF/
    │   │   │       ├── Context/
    │   │   │       ├── Mappings/
    │   │   │       │   └── MapeamentoAquisicao.cs
    │   │   │       └── Migrations/
    │   │   ├── Repositories/
    │   │   └── IoC/
    │   │
    │   └── Api/
    │       ├── Contracts/
    │       │   └── Aquisicoes/
    │       └── Controllers/
    │           └── AquisicoesController.cs
    │
    │
    └── Logging/
        │
        ├── Domain/
        │   └── Entities/
        │       ├── LogUsuario.cs
        │       └── LogJogo.cs
        │
        ├── Application/
        │   ├── Abstractions/
        │   ├── Logs/
        │   └── IoC/
        │
        ├── Infrastructure/
        │   ├── Data/
        │   │   └── EF/
        │   │       ├── Context/
        │   │       ├── Mappings/
        │   │       │   ├── MapeamentoLogUsuario.cs
        │   │       │   └── MapeamentoLogJogo.cs
        │   │       └── Migrations/
        │   └── IoC/
        │
        └── Api/
            ├── Contracts/
            └── Controllers/

```

## 2. Observações do projeto atual

O projeto já apresenta os fundamentos da arquitetura modular:
- separação entre domínio, aplicação, infraestrutura e API;
- uso de `Program.cs` para registro das dependências;
- organização por namespaces e pastas por funcionalidade;
- uso de repositórios e `DbContext` para persistência;
- presença de módulos de `Identity`, `Catalog`, `AccessControl` e `Acquisition` em diferentes níveis do código.
