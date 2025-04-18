# SQL Generator App Basic CRUD - Code vibe

## Overview
SQL Generator App is a powerful tool designed to simplify the process of creating database-first applications. It generates complete, ready-to-use code for data models, repositories, services, controllers, and SQL scripts from your database table definitions.

## Features
- **Complete Code Generation**: Creates all necessary components for a modern layered architecture:
  - Data models with DataContract attributes
  - Repository pattern implementation
  - Service layer with interface
  - Web API controllers (.NET Framework or .NET Core)
  - SQL scripts for CRUD operations

- **Two Framework Options**:
  - .NET Framework (using ApiController)
  - .NET Core (using ControllerBase)

- **Intelligent SQL Scripts**:
  - CREATE TABLE statements
  - Combined SAVE procedure (handles both INSERT and UPDATE)
  - GetAll, GetById, GetByPage stored procedures
  - Delete stored procedure

- **Advanced Features**:
  - Pagination support
  - Search functionality
  - Proper error handling
  - Clean architecture patterns

## How to Use
1. **Define Your Model**:
   - Enter the model name
   - Add fields with their data types, properties, and constraints
   - Mark primary keys and identity fields

2. **Configure Options**:
   - Choose between .NET Framework or .NET Core

3. **Generate Code**:
   - Click "Generate Code" and select the output directory
   - Wait while the app creates all files
   - Review the generated SQL in the preview area

4. **Implementation**:
   - Copy the generated files to your project
   - Execute the SQL scripts in your database
   - Configure your connection string

## Generated Files Structure
```
ModelName/
├── Models/
│   └── ModelNameModel.cs
├── Data/
│   ├── IModelNameRepository.cs
│   ├── ModelNameRepository.cs
│   └── StoredProcedures.cs
├── Services/
│   ├── IModelNameService.cs
│   └── ModelNameService.cs
├── Controllers/
│   └── ModelNameController.cs
├── Common/
│   ├── ResponseModels.cs
│   ├── Extensions.cs
│   └── LogExeption.cs
└── SQL/
    ├── ModelName_CreateTable.sql
    ├── ModelName_Save_Procedure.sql
    ├── ModelName_Delete_Procedure.sql
    ├── ModelName_GetAll_Procedure.sql
    ├── ModelName_GetById_Procedure.sql
    └── ModelName_GetByPage_Procedure.sql
```

## Requirements
- .NET Framework 4.5+ or .NET Core 3.1+
- Microsoft SQL Server database
- Visual Studio 2017+ (recommended)

## Best Practices
- Generated code follows industry standards for clean architecture
- Proper separation of concerns between layers
- Consistent error handling across the application
- Standardized response formats
- Efficient SQL operations with pagination

## License
This software is provided under the MIT license.

## Troubleshooting
If you encounter any issues:
- Ensure your database connection is properly configured
- Verify that all field types are correctly defined
- Check that primary keys are properly designated
- Review generated SQL scripts for any database-specific adjustments 
