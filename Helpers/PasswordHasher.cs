using System;
using System.Security.Cryptography;

namespace TurneroDentalApi.Helpers
{
    // Guarda las contraseñas como "pbkdf2$iteraciones$sal$hash" (PBKDF2 con SHA-256).
    public static class PasswordHasher
    {
        private const string Prefijo = "pbkdf2";
        private const int Iteraciones = 100000;
        private const int TamanoSal = 16;
        private const int TamanoHash = 32;

        public static string Hashear(string contrasena)
        {
            var sal = new byte[TamanoSal];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(sal);

            var hash = Derivar(contrasena, sal, Iteraciones);
            return string.Join("$", Prefijo, Iteraciones, Convert.ToBase64String(sal), Convert.ToBase64String(hash));
        }

        public static bool EsHash(string guardada) =>
            guardada != null && guardada.StartsWith(Prefijo + "$");

        // Acepta hashes y también contraseñas viejas en texto plano (usuarios cargados a mano en la base).
        public static bool Verificar(string contrasena, string guardada)
        {
            if (contrasena == null || guardada == null) return false;
            if (!EsHash(guardada)) return guardada == contrasena;

            var partes = guardada.Split('$');
            if (partes.Length != 4) return false;

            int iteraciones;
            if (!int.TryParse(partes[1], out iteraciones)) return false;

            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var actual = Derivar(contrasena, sal, iteraciones);
            return CompararSeguro(esperado, actual);
        }

        private static byte[] Derivar(string contrasena, byte[] sal, int iteraciones)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(contrasena, sal, iteraciones, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(TamanoHash);
        }

        private static bool CompararSeguro(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diferencia = 0;
            for (int i = 0; i < a.Length; i++) diferencia |= a[i] ^ b[i];
            return diferencia == 0;
        }
    }
}
