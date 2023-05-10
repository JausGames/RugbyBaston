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
            if (curveBinding.propertyName.Contains("RootT.x")) currentRootMotionData.curveX = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootT.y")) currentRootMotionData.curveY = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            if (curveBinding.propertyName.Contains("RootT.z")) currentRootMotionData.curveZ = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            //if (curveBinding.propertyName.Contains("RootQ.x")) boneCurves[0].curve[3] = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            //if (curveBinding.propertyName.Contains("RootQ.y")) boneCurves[0].curve[4] = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            //if (curveBinding.propertyName.Contains("RootQ.z")) boneCurves[0].curve[5] = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
            //if (curveBinding.propertyName.Contains("RootQ.w")) boneCurves[0].curve[6] = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBinding);
        }
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
    public AnimationCurve curveX;
    public AnimationCurve curveY;
    public AnimationCurve curveZ;
    public float length;
    public float speed;
}
