using System;
using System.Collections.Generic;
using System.Reflection;
using Sabresaurus.Sidekick.Responses;
using UnityEngine;

namespace Sabresaurus.Sidekick.Requests
{
    public class GetGameObjectRequest : BaseRequest
    {
        public const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public GetGameObjectRequest(string gameObjectPath)
        {
            GetGameObjectResponse getGOResponse = new GetGameObjectResponse();

            Transform foundTransform = TransformHelper.GetFromPath(gameObjectPath);
            getGOResponse.GameObjectName = foundTransform.name;

            Component[] components = foundTransform.GetComponents<Component>();
            getGOResponse.Components = new List<ComponentDescription>(components.Length);
            Debug.Log("a1");
            foreach (Component component in components)
            {
                Debug.Log("a2 " + component.name);

                InstanceIDMap.AddObject(component);

                ComponentDescription description = new ComponentDescription();
                Type componentType = component.GetType();
                description.TypeName = componentType.Name;
                description.InstanceID = component.GetInstanceID();

                FieldInfo[] fields = componentType.GetFields(BINDING_FLAGS);
                foreach (FieldInfo field in fields)
                {
                    string fieldName = field.Name;
                    Debug.Log("a3 " + field.Name);

                    object objectValue = field.GetValue(component);
                    WrappedVariable wrappedVariable = new WrappedVariable(fieldName, objectValue, field.FieldType);
                    description.Fields.Add(wrappedVariable);
                }

                PropertyInfo[] properties = componentType.GetProperties(BINDING_FLAGS);
                foreach (PropertyInfo property in properties)
                {
                    Debug.Log(" a4 " + property.Name);

                    if (property.DeclaringType == typeof(Component)
                    || property.DeclaringType == typeof(UnityEngine.Object))
                    {
                        continue;
                    }
                    string propertyName = property.Name;
                    MethodInfo getMethod = property.GetGetMethod(true);
                    MethodInfo setMethod = property.GetSetMethod(true);

                    object objectValue = getMethod.Invoke(component, null);
                    WrappedVariable wrappedVariable = new WrappedVariable(propertyName, objectValue, property.PropertyType);
                    description.Properties.Add(wrappedVariable);
                }


                getGOResponse.Components.Add(description);
			
                Debug.Log(" a5 ");
            }
            base.uncastResponse = getGOResponse;
        }
    }
}
