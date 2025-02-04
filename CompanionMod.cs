using MelonLoader;
using StellarModdingToolkit.StellarDriveIntegration;
using Ship.Interface.Settings;
using UnityEngine;
using Ship.Interface.Model.Parts;
using Ship.Parts.Common;
using System.Linq;
using StellarModdingToolkit.Assets;
using System;

using Object = UnityEngine.Object;
using Harmony;

namespace Companion;

public class CompanionMod : MelonMod
{
    public static MelonPreferences_Category PreferenceCategory { get; set; } = MelonPreferences.CreateCategory("CompanionMod");
    public static MelonPreferences_Entry<ushort> PartID { get; set; } = PreferenceCategory.CreateEntry<ushort>("PartID", 0b_10000000_00000000);


    public static AssetLoader? AssetLoader { get; set; }
    public static EventHandler? OnCreatedAssetLoader;


    struct Keys
    {
        public const string CompanionCubeModel = "cc-model";
        public const string CompanionCubeAlbedo = "cc-albedo";
        public const string CompanionCubeNormals = "cc-normal";
        public const string CompanionCubeEmission = "cc-emission";
    }


    public override void OnLateInitializeMelon()
    {
        AssetLoader = new(MelonAssembly.Assembly, LoggerInstance,
        [
            Keys.CompanionCubeModel,
            Keys.CompanionCubeAlbedo,
            Keys.CompanionCubeNormals,
            Keys.CompanionCubeEmission
        ]);
        OnCreatedAssetLoader?.Invoke(this, EventArgs.Empty);
    }


    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (buildIndex != 1) return;
        if (IntegrationUtilities.IsPartIDTaken(PartID.Value))
        {
            LoggerInstance.Warning($"Part ID({PartID.Value}) is already taken!");
            return;
        }

        PartSettings part = ScriptableObject.CreateInstance<PartSettings>();
        part.fullLabel = "Comapnion";
        part.name = "Companion Cube";
        part.description = "Woman's best friend! (sorry dogs)";
        part.internalStateType = Ship.Interface.Model.Parts.StateTypes.PartInternalStateType.None;
        part.size = Vector3.one;
        part.thumbnailTex = Texture2D.blackTexture;
        part.snappingStyle = Ship.Interface.Model.SnappingStyle.PreciseOnFloor;
        part.id = PartID.Value;
        part.buildingCost = [];
        part.part = CreateCube();

        IntegrationUtilities.AddPart(part);
    }

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        if (buildIndex != 1) return;

        foreach (var item in Object.FindObjectsOfType<PartSettings>().Where(p => p.id == PartID.Value).ToArray())
        {
            Object.Destroy(item);
        }
    }


    private GameObject CreateCube()
    {
        float size = 0.8f;

        Vector3 scale = Vector3.one * size;
        Vector3 offset = Vector3.up * (scale.y / 2);

        GameObject partObject = new("Cube");
        var bounds = partObject.AddComponent<ShipPartBounds>();
        bounds.bounds = scale;
        bounds.center = offset;

        GameObject visualContainer = new("Visuals");
        visualContainer.AddComponent<DefaultShipPartVisuals>();
        visualContainer.transform.parent = partObject.transform;

        GameObject cube = new();
        Shader shader = Shader.Find("Shader Graphs/PlanetObjectDefaultLighting");
        Material material = new(shader);

        for (int i = 0; i <= 6; i++)
        {
            foreach (var p in material.GetPropertyNames((MaterialPropertyType)i)) LoggerInstance.Msg(p);
        }

        cube.AddComponent<MeshFilter>().mesh = AssetLoader.GetAsset<Mesh>(Keys.CompanionCubeModel);
        cube.AddComponent<MeshRenderer>().material = material;

        material.SetTexture("_AlbedoTex", AssetLoader.GetAsset<Texture2D>(Keys.CompanionCubeAlbedo));
        material.SetTexture("_EmissionTex", AssetLoader.GetAsset<Texture2D>(Keys.CompanionCubeEmission));
        material.SetTexture("_NormalMap", AssetLoader.GetAsset<Texture2D>(Keys.CompanionCubeNormals));
        material.SetColor("_EmissionColor", new Color(174 / 255f, 77/255f, 129/255f) * 1.5f);

        cube.transform.parent = visualContainer.transform;
        cube.transform.localScale = scale * 50;
        cube.transform.localPosition = offset;
        cube.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        partObject.SetActive(false);

        return partObject;
    }
}
