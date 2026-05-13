using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
public class SetupHeroSprites : MonoBehaviour
{
    [MenuItem("Joc 2D/Pune Skinul Hero pe Player")]
    public static void Setup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null) return;

        // 1. Scalare
        player.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // 2. Animator Controller
        string controllerPath = "Assets/Animations/HeroAnimator.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        
        if (controller == null) {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        // ASIGURAM PARAMETRII (Sa nu mai dea erori in consola)
        if (!controller.parameters.Any(p => p.name == "isMoving")) controller.AddParameter("isMoving", AnimatorControllerParameterType.Bool);
        if (!controller.parameters.Any(p => p.name == "shoot")) controller.AddParameter("shoot", AnimatorControllerParameterType.Trigger);
        if (!controller.parameters.Any(p => p.name == "damage")) controller.AddParameter("damage", AnimatorControllerParameterType.Trigger);
        if (!controller.parameters.Any(p => p.name == "dead")) controller.AddParameter("dead", AnimatorControllerParameterType.Trigger);

        Animator anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        Debug.Log("GATA! Parametrii animatorului au fost adaugati. Erorile vor disparea.");
    }
}
#endif
