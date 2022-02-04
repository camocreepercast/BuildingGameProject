using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTile : MonoBehaviour
{
	public SO_TileType tileType;
	public ContainedRoom roomContainer;
	public RoomTile[] neighborRooms;
	public bool[] neighborWelds; //up right down left
	public long previousUpdateID;
	public SpriteRenderer spriteRenderer;

	[HideInInspector] public readonly Vector2Int[] offsets = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
	[HideInInspector] public readonly int[] inverseOffsets = { 2, 3, 0, 1 };
	public void UpdateTile()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = tileType.backgroundSprite;

		UpdateNeighboringTiles();
	}
	public void UpdateNeighboringTiles()
	{
		neighborRooms = new RoomTile[4];
		for (int i = 0; i < 4; i++)
		{
			RoomTile rt = roomContainer.globalRefManager.baseManager.GetRoomAtPosition(GetTrueTilePosition() + offsets[i]);
			if (rt != null)
			{
				rt.neighborRooms[inverseOffsets[i]] = this;
				neighborRooms[i] = rt;

				//@Start tile updates here
				if (roomContainer.isNaturalTerrainTile)
				{
					//figure out the dirt shit
				}
			}
		}
	}

	public Vector2Int GetTrueTilePosition()
	{
		return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
	}
	public Vector2Int GetIndexdTilePosition()
	{
		return new Vector2Int(Mathf.RoundToInt(transform.position.x + (roomContainer.globalRefManager.terrainManager.terrainWidth / 2)), Mathf.RoundToInt(transform.position.y + roomContainer.globalRefManager.terrainManager.terrainBottomLayer));
	}
}

