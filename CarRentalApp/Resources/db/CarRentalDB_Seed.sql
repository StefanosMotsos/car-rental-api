DO $$
BEGIN

INSERT INTO "Roles" ("Name")
VALUES ('ADMIN'), ('EMPLOYEE'), ('CUSTOMER');

INSERT INTO "Capabilities" ("Name", "Description")
VALUES
    ('INSERT_EMPLOYEE',   'Create a new employee'),
    ('VIEW_EMPLOYEES',    'View employee list and details'),
    ('VIEW_EMPLOYEE',     'View employee'),
    ('EDIT_EMPLOYEE',     'Modify existing employee'),
    ('DELETE_EMPLOYEE',   'Remove an employee'),
    ('VIEW_ONLY_EMPLOYEE','View only own employee details'),
    ('INSERT_CUSTOMER',   'Create a new customer'),
    ('VIEW_CUSTOMERS',    'View customer list and details'),
    ('VIEW_CUSTOMER',     'View customer'),
    ('EDIT_CUSTOMER',     'Modify existing customer'),
    ('DELETE_CUSTOMER',   'Remove a customer'),
    ('VIEW_ONLY_CUSTOMER','View only own customer details'),
    ('INSERT_VEHICLE',    'Create a new vehicle'),
    ('VIEW_VEHICLES',     'View vehicle list and details'),
    ('VIEW_VEHICLE',      'View vehicle'),
    ('EDIT_VEHICLE',      'Modify existing vehicle'),
    ('DELETE_VEHICLE',    'Remove a vehicle'),
    ('INSERT_RENTAL',     'Create a new rental'),
    ('VIEW_RENTALS',      'View rental list and details'),
    ('VIEW_RENTAL',       'View rental'),
    ('EDIT_RENTAL',       'Modify existing rental'),
    ('DELETE_RENTAL',     'Remove a rental'),
    ('VIEW_ONLY_RENTAL',  'View only own rental history'),
    ('APPROVE_RENTAL',    'Approve or reject a rental request');

INSERT INTO "RolesCapabilities" ("RolesId", "CapabilitiesId")
SELECT r."Id", c."Id"
FROM "Roles" r
CROSS JOIN "Capabilities" c
WHERE r."Name" = 'ADMIN';

INSERT INTO "RolesCapabilities" ("RolesId", "CapabilitiesId")
SELECT r."Id", c."Id"
FROM "Roles" r
CROSS JOIN "Capabilities" c
WHERE r."Name" = 'EMPLOYEE'
  AND c."Name" IN (
      'VIEW_CUSTOMERS', 'VIEW_CUSTOMER',
      'VIEW_VEHICLES',  'VIEW_VEHICLE',
      'INSERT_VEHICLE',
      'VIEW_RENTALS',   'VIEW_RENTAL',
      'APPROVE_RENTAL'
  );

INSERT INTO "RolesCapabilities" ("RolesId", "CapabilitiesId")
SELECT r."Id", c."Id"
FROM "Roles" r
CROSS JOIN "Capabilities" c
WHERE r."Name" = 'CUSTOMER'
  AND c."Name" IN (
      'VIEW_ONLY_CUSTOMER',
      'VIEW_VEHICLES', 'VIEW_VEHICLE',
      'INSERT_RENTAL', 'VIEW_ONLY_RENTAL'
  );

INSERT INTO "Locations" ("Name", "Address", "City", "Phone")
VALUES
    ('Athens Center',        'Syntagma Square 1',       'Athens',       '2101234567'),
    ('Piraeus Port',         'Akti Miaouli 10',         'Piraeus',      '2104567890'),
    ('Thessaloniki Airport', 'Makedonia Airport Rd 1',  'Thessaloniki', '2310123456'),
    ('Glyfada',              'Glyfada Ave 45',          'Athens',       '2109876543'),
    ('Kolonaki',             'Patriarchou Ioakeim 12',  'Athens',       '2107654321'),
    ('Kallithea',            'Thisseos 80',             'Athens',       '2109871234'),
    ('Kifisia',              'Kolokotroni 5',           'Athens',       '2108765432'),
    ('Thessaloniki Center',  'Tsimiski 22',             'Thessaloniki', '2310654321'),
    ('Kalamaria',            'Megalou Alexandrou 3',    'Thessaloniki', '2310987654'),
    ('Peristeri',            'Thivon 120',              'Athens',       '2105671234');

INSERT INTO "Categories" ("Name", "Description")
VALUES
    ('Sedan',        'Classic four-door sedan'),
    ('SUV',          'Sport utility vehicle'),
    ('Convertible',  'Open-top convertible'),
    ('Minivan',      'Family minivan'),
    ('Coupe',        'Two-door coupe'),
    ('Pickup Truck', 'Light duty pickup truck'),
    ('Luxury',       'High-end luxury vehicle'),
    ('Electric',     'Fully electric vehicle');

SELECT setval(pg_get_serial_sequence('"Roles"',       'Id'), 3);
SELECT setval(pg_get_serial_sequence('"Capabilities"','Id'), 24);
SELECT setval(pg_get_serial_sequence('"Locations"',   'Id'), 10);
SELECT setval(pg_get_serial_sequence('"Categories"',  'Id'), 8);

END $$;