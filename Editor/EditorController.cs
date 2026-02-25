using UnityEditor;
using Unity.BestPractices;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    public static class EditorController
    {
        [MenuItem("Window/Best Practices/Hello editor")]
        public static void LogHelloMenu()
        {
			Debug.Log("Hello from Editor. Activated by best practice menu!");
        }

		[MenuItem("Window/Best Practices/Hello editor using package")]
        public static void LogHelloFromRuntime()
        {
			RuntimeController.LogHelloMenu();
        }
    }
}
