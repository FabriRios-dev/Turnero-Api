-- Turnero Dental: tablas para horarios de atención y consultas del formulario de contacto.
-- El script se puede ejecutar más de una vez sin duplicar datos.
USE TurneroDentalWeb;
GO

IF OBJECT_ID('dbo.HorarioAtencion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HorarioAtencion (
        IdHorario        INT IDENTITY(1,1) PRIMARY KEY,
        IdProfesional    INT        NOT NULL,
        DiaSemana        TINYINT    NOT NULL,           -- 0 = domingo ... 6 = sábado (igual que DayOfWeek de .NET)
        HoraInicio       VARCHAR(5) NOT NULL,           -- formato HH:mm
        HoraFin          VARCHAR(5) NOT NULL,
        DuracionMinutos  INT        NOT NULL CONSTRAINT DF_HorarioAtencion_Duracion DEFAULT 30,
        CONSTRAINT FK_HorarioAtencion_Profesional FOREIGN KEY (IdProfesional)
            REFERENCES dbo.Profesional (IdProfesional) ON DELETE CASCADE,
        CONSTRAINT CK_HorarioAtencion_Dia CHECK (DiaSemana BETWEEN 0 AND 6),
        CONSTRAINT CK_HorarioAtencion_Rango CHECK (HoraInicio < HoraFin),
        CONSTRAINT CK_HorarioAtencion_Duracion CHECK (DuracionMinutos BETWEEN 10 AND 240)
    );
END
GO

IF OBJECT_ID('dbo.Consulta', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consulta (
        IdConsulta  INT IDENTITY(1,1) PRIMARY KEY,
        Nombre      VARCHAR(100)  NOT NULL,
        Email       VARCHAR(100)  NOT NULL,
        Telefono    VARCHAR(20)   NULL,
        Mensaje     VARCHAR(1000) NOT NULL,
        FechaEnvio  DATETIME      NOT NULL CONSTRAINT DF_Consulta_FechaEnvio DEFAULT GETDATE(),
        Leida       BIT           NOT NULL CONSTRAINT DF_Consulta_Leida DEFAULT 0
    );
END
GO

-- Horarios de ejemplo para los profesionales cargados.
IF NOT EXISTS (SELECT 1 FROM dbo.HorarioAtencion)
BEGIN
    -- Marina López (Odontología general): lunes a viernes 08:00 a 12:00
    INSERT INTO dbo.HorarioAtencion (IdProfesional, DiaSemana, HoraInicio, HoraFin, DuracionMinutos)
    SELECT 1, d, '08:00', '12:00', 30 FROM (VALUES (1),(2),(3),(4),(5)) AS dias(d)
    WHERE EXISTS (SELECT 1 FROM dbo.Profesional WHERE IdProfesional = 1);

    -- Ezequiel Torres (Ortodoncia): lunes, miércoles y viernes 14:00 a 18:00
    INSERT INTO dbo.HorarioAtencion (IdProfesional, DiaSemana, HoraInicio, HoraFin, DuracionMinutos)
    SELECT 2, d, '14:00', '18:00', 30 FROM (VALUES (1),(3),(5)) AS dias(d)
    WHERE EXISTS (SELECT 1 FROM dbo.Profesional WHERE IdProfesional = 2);

    -- Sofía Ramírez (Endodoncia): martes y jueves 09:00 a 13:00, sábados 09:00 a 12:00
    INSERT INTO dbo.HorarioAtencion (IdProfesional, DiaSemana, HoraInicio, HoraFin, DuracionMinutos)
    SELECT 3, d, '09:00', '13:00', 30 FROM (VALUES (2),(4)) AS dias(d)
    WHERE EXISTS (SELECT 1 FROM dbo.Profesional WHERE IdProfesional = 3);

    INSERT INTO dbo.HorarioAtencion (IdProfesional, DiaSemana, HoraInicio, HoraFin, DuracionMinutos)
    SELECT 3, 6, '09:00', '12:00', 30
    WHERE EXISTS (SELECT 1 FROM dbo.Profesional WHERE IdProfesional = 3);
END
GO

-- Usuario administrador para las pantallas de gestión (Pacientes, Profesionales, Turnos).
-- La contraseña inicial queda en texto plano y la API la convierte a hash en el primer inicio de sesión.
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE NombreUsuario = 'admin')
    INSERT INTO dbo.Usuario (NombreUsuario, Contrasena, Rol) VALUES ('admin', 'admin123', 'Admin');
GO
