using UnityEngine;
using System.Collections;

/// <summary>
/// Author: 		Sebastiaan Fehr (Seb@TheBinaryMill.com)
/// Date: 			March 2013
/// Summary:		Creates MonoBehaviour instance through which 
///                 static classes can call StartCoroutine.
/// Description:    Classes that do not inherit from MonoBehaviour, or static 
///                 functions within MonoBehaviours are inertly unable to 
///                 call StartCoroutene, as this function is not static and 
///                 does not exist on Object. This Class creates a proxy though
///                 which StartCoroutene can be called, and destroys it when 
///                 no longer needed.
/// </summary>
public class Coroutiner
{
    private static GameObject routeneHandlerGo = new GameObject("Coroutiner");
    private static CoroutinerInstance routeneHandler = routeneHandlerGo.AddComponent(typeof(CoroutinerInstance)) as CoroutinerInstance;
    public static Coroutine StartCoroutine(IEnumerator iterationResult)
    {
        return routeneHandler.ProcessWork(iterationResult);
    }

}

public class CoroutinerInstance : MonoBehaviour
{

    public Coroutine ProcessWork(IEnumerator iterationResult)
    {
        return StartCoroutine(iterationResult);
    }

}
