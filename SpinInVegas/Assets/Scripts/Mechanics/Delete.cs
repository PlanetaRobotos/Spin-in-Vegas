using System;
using System.Collections;
using UnityEngine;

namespace Mechanics
{
    public class Delete : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(4f);
            Destroy(gameObject);
        }
    }
}