IF DB_ID(N'VeterinariaDb') IS NULL CREATE DATABASE VeterinariaDb;
GO
USE VeterinariaDb;
GO
IF OBJECT_ID(N'dbo.Duenos', N'U') IS NULL
CREATE TABLE dbo.Duenos (
    DuenoID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Duenos PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    Correo nvarchar(150) NOT NULL,
    Telefono nvarchar(20) NOT NULL,
    CONSTRAINT UQ_Duenos_Correo UNIQUE (Correo)
);
IF OBJECT_ID(N'dbo.Mascotas', N'U') IS NULL
CREATE TABLE dbo.Mascotas (
    MascotaID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mascotas PRIMARY KEY,
    Nombre nvarchar(80) NOT NULL,
    Especie nvarchar(40) NOT NULL,
    Raza nvarchar(60) NOT NULL,
    FechaNacimiento date NOT NULL,
    DuenoID int NOT NULL,
    CONSTRAINT FK_Mascotas_Duenos FOREIGN KEY (DuenoID) REFERENCES dbo.Duenos(DuenoID) ON DELETE NO ACTION
);
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Duenos WHERE Correo = N'ana@example.com')
    INSERT dbo.Duenos (Nombre, Correo, Telefono) VALUES (N'Ana Torres', N'ana@example.com', N'999111222');
IF NOT EXISTS (SELECT 1 FROM dbo.Duenos WHERE Correo = N'luis@example.com')
    INSERT dbo.Duenos (Nombre, Correo, Telefono) VALUES (N'Luis Pérez', N'luis@example.com', N'999333444');
IF NOT EXISTS (SELECT 1 FROM dbo.Mascotas WHERE Nombre = N'Luna' AND DuenoID = (SELECT DuenoID FROM dbo.Duenos WHERE Correo = N'ana@example.com'))
    INSERT dbo.Mascotas (Nombre, Especie, Raza, FechaNacimiento, DuenoID) VALUES (N'Luna', N'Perro', N'Labrador', '2021-04-12', (SELECT DuenoID FROM dbo.Duenos WHERE Correo = N'ana@example.com'));
IF NOT EXISTS (SELECT 1 FROM dbo.Mascotas WHERE Nombre = N'Milo' AND DuenoID = (SELECT DuenoID FROM dbo.Duenos WHERE Correo = N'ana@example.com'))
    INSERT dbo.Mascotas (Nombre, Especie, Raza, FechaNacimiento, DuenoID) VALUES (N'Milo', N'Gato', N'Criollo', '2022-08-03', (SELECT DuenoID FROM dbo.Duenos WHERE Correo = N'ana@example.com'));
IF NOT EXISTS (SELECT 1 FROM dbo.Mascotas WHERE Nombre = N'Rocky' AND DuenoID = (SELECT DuenoID FROM dbo.Duenos WHERE Correo = N'luis@example.com'))
    INSERT dbo.Mascotas (Nombre, Especie, Raza, FechaNacimiento, DuenoID) VALUES (N'Rocky', N'Perro', N'Beagle', '2020-01-15', (SELECT DuenoID FROM dbo.Duenos WHERE Correo = N'luis@example.com'));
GO
