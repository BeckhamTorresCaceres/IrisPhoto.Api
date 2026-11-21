namespace IrisPhoto_Api.Domain.Enums
{
    public enum PasswordType
    {
        /// <summary>
        /// Contraseña alfanumérica tradicional.
        /// </summary>
        Password = 1,

        /// <summary>
        /// Código numérico corto (ej. 4 a 6 dígitos).
        /// </summary>
        Pin = 2,

        /// <summary>
        /// Patrón de desbloqueo por secuencia de nodos (ej. "0-1-2-5-8").
        /// </summary>
        Pattern = 3
    }
}
