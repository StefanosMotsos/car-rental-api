BEGIN TRY
    BEGIN TRANSACTION;

    -- ============================================
    -- CarRentalDB - Seed Data
    -- Roles, Capabilities, Role-Capability mappings
    -- ============================================

    -- ============================================
    -- Insert Roles
    -- ============================================
    INSERT INTO [dbo].[Roles] ([Name])
    VALUES
        ('ADMIN'),
        ('EMPLOYEE'),
        ('CUSTOMER');

    -- ============================================
    -- Insert Capabilities
    -- ============================================
    INSERT INTO [dbo].[Capabilities] ([Name], [Description])
    VALUES
        ('INSERT_EMPLOYEE', 'Create a new employee'),
        ('VIEW_EMPLOYEES', 'View employee list and details'),
        ('VIEW_EMPLOYEE', 'View employee'),
        ('EDIT_EMPLOYEE', 'Modify existing employee'),
        ('DELETE_EMPLOYEE', 'Remove an employee'),
        ('VIEW_ONLY_EMPLOYEE', 'View only own employee details'),
        ('INSERT_CUSTOMER', 'Create a new customer'),
        ('VIEW_CUSTOMERS', 'View customer list and details'),
        ('VIEW_CUSTOMER', 'View customer'),
        ('EDIT_CUSTOMER', 'Modify existing customer'),
        ('DELETE_CUSTOMER', 'Remove a customer'),
        ('VIEW_ONLY_CUSTOMER', 'View only own customer details'),
        ('INSERT_VEHICLE', 'Create a new vehicle'),
        ('VIEW_VEHICLES', 'View vehicle list and details'),
        ('VIEW_VEHICLE', 'View vehicle'),
        ('EDIT_VEHICLE', 'Modify existing vehicle'),
        ('DELETE_VEHICLE', 'Remove a vehicle'),
        ('INSERT_RENTAL', 'Create a new rental'),
        ('VIEW_RENTALS', 'View rental list and details'),
        ('VIEW_RENTAL', 'View rental'),
        ('EDIT_RENTAL', 'Modify existing rental'),
        ('DELETE_RENTAL', 'Remove a rental'),
        ('VIEW_ONLY_RENTAL', 'View only own rental history'),
        ('APPROVE_RENTAL', 'Approve or reject a rental request');

    -- ============================================
    -- ADMIN: all capabilities
    -- ============================================
    INSERT INTO [dbo].[RolesCapabilities] ([RolesId], [CapabilitiesId])
    SELECT r.[Id], c.[Id]
    FROM [dbo].[Roles] r
    CROSS JOIN [dbo].[Capabilities] c
    WHERE r.[Name] = 'ADMIN';

    -- ============================================
    -- EMPLOYEE: VIEW_CUSTOMERS, VIEW_CUSTOMER,
    --           VIEW_VEHICLES, VIEW_VEHICLE,
    --           INSERT_VEHICLE,
    --           VIEW_RENTALS, VIEW_RENTAL,
    --           APPROVE_RENTAL
    -- ============================================
    INSERT INTO [dbo].[RolesCapabilities] ([RolesId], [CapabilitiesId])
    SELECT r.[Id], c.[Id]
    FROM [dbo].[Roles] r
    CROSS JOIN [dbo].[Capabilities] c
    WHERE r.[Name] = 'EMPLOYEE'
      AND c.[Name] IN (
          'VIEW_CUSTOMERS', 'VIEW_CUSTOMER',
          'VIEW_VEHICLES', 'VIEW_VEHICLE',
          'INSERT_VEHICLE',
          'VIEW_RENTALS', 'VIEW_RENTAL',
          'APPROVE_RENTAL'
      );

    -- ============================================
    -- CUSTOMER: VIEW_ONLY_CUSTOMER,
    --           VIEW_VEHICLES, VIEW_VEHICLE,
    --           INSERT_RENTAL, VIEW_ONLY_RENTAL
    -- ============================================
    INSERT INTO [dbo].[RolesCapabilities] ([RolesId], [CapabilitiesId])
    SELECT r.[Id], c.[Id]
    FROM [dbo].[Roles] r
    CROSS JOIN [dbo].[Capabilities] c
    WHERE r.[Name] = 'CUSTOMER'
      AND c.[Name] IN (
          'VIEW_ONLY_CUSTOMER',
          'VIEW_VEHICLES', 'VIEW_VEHICLE',
          'INSERT_RENTAL', 'VIEW_ONLY_RENTAL'
      );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;

DBCC CHECKIDENT ('dbo.Roles', RESEED, 3);
DBCC CHECKIDENT ('dbo.Capabilities', RESEED, 24);