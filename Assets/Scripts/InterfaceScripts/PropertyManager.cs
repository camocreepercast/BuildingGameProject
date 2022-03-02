using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyManager : MonoBehaviour
{
	public GlobalRefManager globalRefManager;
	public GameObject propertyTagPrefab;
	public List<Color> propertyColours;
	public List<Sprite> propertyIcons;
	public Dictionary<string, SO_Property> propertyTagLookup;
	public Dictionary<string, PropertyDisplayer> propertyDisplays;
	public SO_Property defaultProperty;

	private void Awake()
	{
		SO_Property[] unsorted = Resources.LoadAll<SO_Property>("");
		propertyTagLookup = new Dictionary<string, SO_Property>();
		foreach(SO_Property property in unsorted)
		{
			string key = ("prop_" + property.propertyType + "_" + property.callbackID).ToLower();
			if (propertyTagLookup.ContainsKey(key))
			{
				propertyTagLookup[key] = property;
			}
			else
			{
				propertyTagLookup.Add(key, property);
			}
		}

#pragma	warning disable
		PropertyDisplayer[] allDisplayers = (PropertyDisplayer[])FindObjectsOfTypeAll(typeof(PropertyDisplayer));
#pragma warning enable
		propertyDisplays = new Dictionary<string, PropertyDisplayer>();
		foreach(PropertyDisplayer displayer in allDisplayers)
		{
			if (propertyDisplays.ContainsKey(displayer.propertyDisplayCallbackID.ToLower()))
				propertyDisplays[displayer.propertyDisplayCallbackID.ToLower()] = displayer;
			else
				propertyDisplays.Add(displayer.propertyDisplayCallbackID.ToLower(), displayer);
		}
	}

	public enum PropertyType
	{
		None,
		Species,
		Resource,
		Biome,
		Style,
		Rarity,
		Age,
	}

	/// <summary>
	/// Get the biome from the key
	/// </summary>
	/// <param name="callbackID">The string callback ID for the biome</param>
	/// <returns>The biome property, given it exists</returns>
	public SO_Property GetBiome(string callbackID)
	{
		return GetProperty(PropertyType.Biome, callbackID);
	}

	/// <summary>
	/// Get the colour of a property. (Colour values set in inspector)
	/// </summary>
	/// <param name="a">The property type</param>
	/// <returns>The colour set for type 'a'</returns>
	public Color GetColour(PropertyType a)
	{
		return propertyColours[(int)a];
	}

	/// <summary>
	/// Get the icon of a property type. (Sprites set in the inspector)
	/// </summary>
	/// <param name="a">The property type</param>
	/// <returns>The icon sprite for type 'a'</returns>
	public Sprite GetIcon(PropertyType a)
	{
		return propertyIcons[(int)a];
	}

	/// <summary>
	/// Get a property scriptable from the lookup
	/// </summary>
	/// <param name="type">The type of property</param>
	/// <param name="indexID">The callback ID if the property</param>
	/// <returns>The property scriptable, given it exists</returns>
	public SO_Property GetProperty(PropertyType type, string indexID)
	{
		return GetProperty("prop_" + type + "_" + indexID);
	}

	/// <summary>
	/// Get a property scriptable from the lookup
	/// </summary>
	/// <param name="callbackID">The full callback ID of the property</param>
	/// <returns>The property scriptable, given it exists</returns>
	public SO_Property GetProperty(string callbackID)
	{
		return propertyTagLookup.ContainsKey(callbackID.ToLower()) ? propertyTagLookup[callbackID.ToLower()] : null;
	}

	/// <summary>
	/// Get the proprty displayer from the lookup
	/// </summary>
	/// <param name="callbackID">The callback ID of the displayer</param>
	/// <returns>The property displayer, given it exists</returns>
	public PropertyDisplayer GetPropertyDisplayer(string callbackID)
	{
		return propertyDisplays.ContainsKey(callbackID.ToLower()) ? propertyDisplays[callbackID.ToLower()] : null;
	}

}
