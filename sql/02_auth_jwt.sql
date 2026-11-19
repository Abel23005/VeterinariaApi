USE VeterinariaDb;
GO
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Correo nvarchar(150) NOT NULL,
        ClaveHash nvarchar(200) NOT NULL,
        Rol nvarchar(40) NOT NULL,
        CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo),
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'Recepcionista', N'Veterinario'))
    );
END;
GO
-- Clave de demostración para ambos registros: VetCare123! (cambiar después de probar).
-- BCrypt hash; son cuentas locales de laboratorio, no usar en producción.
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo=N'recepcion@example.com')
 INSERT dbo.Usuarios(Correo,ClaveHash,Rol) VALUES (N'recepcion@example.com',N'$2a$11$Qynll2oX5gL2dVZ3gKW7qe4xj.xCAJRJFAq9Bp9j5wIXW.FEn9d4G',N'Recepcionista');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo=N'veterinario@example.com')
 INSERT dbo.Usuarios(Correo,ClaveHash,Rol) VALUES (N'veterinario@example.com',N'$2a$11$Qynll2oX5gL2dVZ3gKW7qe4xj.xCAJRJFAq9Bp9j5wIXW.FEn9d4G',N'Veterinario');
GO
