using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class SerializedPropertyAttribute : PropertyAttribute
{
}

public abstract class SerializedMonoBehaviour :
    MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector] private string SceneGUID;
    [SerializeField, HideInInspector] private string GUID;
    private bool _alreadySerialized;

    public void OnBeforeSerialize()
    {
        DeserializeData();
    }


    public void OnAfterDeserialize()
    {
        DeserializeData();
    }

    private void DeserializeData()
    {
        if (_alreadySerialized is false)
        {
            _alreadySerialized = true;

#if UNITY_EDITOR
            var data = DeserializedDataBase.TempDeserializedData;
#else
            var data = DeserializedDataBase.CachedData;
#endif
            var dataHolder =
                data?.DeserializedDataSet.FirstOrDefault(x => x.SceneGUID == SceneGUID)
                    ?.SerializedMonoDataSet
                    .FirstOrDefault(x => x.GUID == GUID);

            if (dataHolder != null)
            {
                
                foreach (var serializedData in dataHolder.PropertyDatas)
                {
                    var propertyInfo = serializedData.ToProperty();
                    var propertyType = serializedData.ToProperty().PropertyType;
                    var targetValueType = serializedData.Value.GetType();


                    if (propertyType != targetValueType)
                    {
                        if (serializedData.Value is IConvertible)
                        {
                            if (propertyType.BaseType == typeof(Enum))
                            {
                                var value = Enum.Parse(propertyType, serializedData.Value.ToString());
                                propertyInfo.SetValue(this, value);
                            }
                            else
                            {
                                var value = Convert.ChangeType(serializedData.Value, propertyType);
                                propertyInfo.SetValue(this, value);
                            }
                        }
                        else
                        {
                            var deserializeObject = JsonConvert.DeserializeObject(serializedData.Value.ToString(),
                                propertyType);
                            propertyInfo.SetValue(this, deserializeObject);
                        }
                    }

                    else
                    {
                        propertyInfo.SetValue(this, serializedData.Value);
                    }
                }
            }
        }
    }
}