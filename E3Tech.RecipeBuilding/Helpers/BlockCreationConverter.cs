using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using Unity;

namespace E3Tech.RecipeBuilding.Helpers
{
    public class BlockCreationConverter<T> : CustomCreationConverter<T> where T : class
    {
        private IUnityContainer unityContainer;

        public BlockCreationConverter(IUnityContainer unityContainer)
        {
            this.unityContainer = unityContainer;
        }

        public override bool CanConvert(Type objectType)
        {
            return unityContainer.IsRegistered(objectType);
        }

        public override T Create(Type objectType)
        {
            return Activator.CreateInstance<T>();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            
            JObject item = JObject.Load(reader);
            
            switch (item["Name"].Value<string>())
            {
                case "Start":
                    return item.ToObject<ParameterizedRecipeBlock<StartBlockParameters>>();
                case "Stirrer":
                    return item.ToObject<ParameterizedRecipeBlock<StirrerBlockParameters>>();
                case "HeatCool":
                    return item.ToObject<ParameterizedRecipeBlock<HeatCoolBlockParameters>>();
                case "Wait":
                    return item.ToObject<ParameterizedRecipeBlock<WaitBlockParameters>>();
                case "Transfer":
                    return item.ToObject<ParameterizedRecipeBlock<TransferBlockParameters>>();
                case "End":
                    return item.ToObject<ParameterizedRecipeBlock<EndBlockParameters>>();
                default:
                    return item.ToObject<RecipeBlock>();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer);

        }
    }
}
