using UnityEngine.SceneManagement;

namespace Assets._Project.Code.CommonServices
{
    public class SceneLoader
    {
        public void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }
    }
}