using System.Web;
using System.Web.Optimization;

namespace TurneroDentalApi
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        // La API no tiene páginas propias (el frontend es React), así que no hay scripts ni
        // estilos para agrupar. Las carpetas Scripts y Content de la plantilla no existen:
        // registrar uniones que apunten a ellas hace fallar el inicio de la aplicación.
        public static void RegisterBundles(BundleCollection bundles)
        {
        }
    }
}
