using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionExtractor : MonoBehaviour
{
    static public AnimationClipRootMotionData OnAnimationChange(AnimationClip clip, float speed)
    {
        //var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;

        var curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(clip);

        //var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        var currentRootMotionData = new AnimationClipRootMotionData();
        currentRootMotionData.length = clip.length;
        currentRootMotionData.speed = speed;

        foreach (var curveBinding in curveBindings)
        {
            if (curveBinding.propertyName.Contains("RootT.x")) currentRootMotionData.curvePosX = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootT.y")) currentRootMotionData.curvePosY = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootT.z")) currentRootMotionData.curvePosZ = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootQ.x")) currentRootMotionData.curveRotX = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootQ.y")) currentRootMotionData.curveRotY = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootQ.z")) currentRootMotionData.curveRotZ = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootQ.w")) currentRootMotionData.curveRotW = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
        }

        //xRot = currentRootMotionData.curvePosX.Evaluate(clip.length) currentRootMotionData.curveRotX.Evaluate(0)


        return currentRootMotionData;
    }
    public AnimationClip FindAnimation(string name, Animator animator)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }

        return null;
    }
}
public class AnimationClipRootMotionData
{
    public AnimationCurve curvePosX;
    public AnimationCurve curvePosY;
    public AnimationCurve curvePosZ;
    public AnimationCurve curveRotX;
    public AnimationCurve curveRotY;
    public AnimationCurve curveRotZ;
    public AnimationCurve curveRotW;
    public float length;
    public float speed;
}
