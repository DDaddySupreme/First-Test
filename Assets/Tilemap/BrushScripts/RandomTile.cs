using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace UnityEngine.Tilemaps
{

    [Serializable]
    [CreateAssetMenu(fileName = "RandomTileAsset", menuName = "Tilemap/Random", order = 1)]
    public class RandomTile : TileBase
    {
        //public Sprite mainSprite;
        //string path;
        [Tooltip("This is the name of the sprite file")]
        public Sprite[] spriteArray;
        public int[] spriteWeight;

        private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {


        }

        private Sprite GetSprite()
        {
            List<Sprite> tempList = new List<Sprite>();

            for (int n = 0; n < spriteArray.Length; n++)
            {
                //total += num;
                for (int p = 0; p < spriteWeight[n]; p++)
                {
                    tempList.Add(spriteArray[n]);
                }
            }

            return tempList[Random.Range(0, tempList.Count)];
        }

        /*public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            foreach (var p in new BoundsInt(-1, -1, 0, 3, 3, 1).allPositionsWithin)
            {
                tilemap.RefreshTile(position + p);
            }
        }*/

        private bool TileValue(ITilemap tileMap, Vector3Int position)
        {
            TileBase tile = tileMap.GetTile(position);
            return (tile != null && tile == this);
        }

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {
            tileData.transform = Matrix4x4.identity;
            tileData.color = Color.white;

            tileData.sprite = GetSprite();
            tileData.color = Color.white;
            tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
            tileData.colliderType = Tile.ColliderType.Sprite;
        }

        public Sprite GetPreview()
        {
            return spriteArray[0];
        }

    }
}