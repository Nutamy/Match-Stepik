// using System.Collections.Generic;
// using ResourcesLoading;
// using UnityEngine;
// using VContainer;
//
// namespace Game.Utils
// {
//     public class FXPool
//     {
//         private readonly List<GameObject> _FXPool = new List<GameObject>();
//         private GameObject _prefabFX;
//         private IObjectResolver _objectResolver;
//         private readonly GameResourcesLoader _resourcesaLoader;
//
//         public FXPool(IObjectResolver objectResolver, GameResourcesLoader resourcesaLoader)
//         {
//             _objectResolver = objectResolver;
//             _resourcesaLoader = resourcesaLoader;
//         }
//
//         public GameObject GetFXPool(Vector3 position, Transform transform)
//         {
//             for (int i = 0; i < _FXPool.Count; i++)
//             {
//                 if(_FXPool[i].activeInHierarchy) continue;
//                 _FXPool[i].gameObject.transform.position = position;
//                 _FXPool[i].SetActive(true);
//                 return _FXPool[i];
//             }
//         }
//     }
//     
// }