using Assets._Project.Code.CommonServices;

namespace Assets._Project.Code.Infrustructure
{
    public static class GlobalServices
    {
        public static SceneLoader SceneLoader;

        public static void Initialize()
        {
            SceneLoader = new SceneLoader();
        }
    }
}