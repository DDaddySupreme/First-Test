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
    [CreateAssetMenu(fileName = "ConnectedTileAsset", menuName = "Tilemap/Connected", order = 1)]
    public class ConnectedTextures : TileBase
    {
        //public Sprite mainSprite;
        //string path;
        int textureID;
        [Tooltip("This is the name of the sprite file")]
        public string multipleSprite;
        public Sprite[] spriteArray = new Sprite[59];

        private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {


        }

        private int GetIndex(byte num) // Indexes only make any sense with my sprite sheets. Use them for reference
        {
            spriteArray = Resources.LoadAll<Sprite>(multipleSprite);
            switch (num)
            {
                case 0:
                case 1:
                case 4:
                case 5:
                case 32:
                case 33:
                case 36:
                case 37:
                case 128:
                case 129:
                case 132:
                case 133:
                case 160:
                case 161:
                case 164:
                case 165:
                    return 50; // Single Block
                case 31:
                case 63:
                case 159:
                case 191:
                    return 1; // One edge - N
                case 214:
                case 215:
                case 246:
                case 247:
                    return 10; // One edge - E
                case 248:
                case 249:
                case 252:
                case 253:
                    return 17; // One edge - S
                case 107:
                case 111:
                case 235:
                case 239:
                    return 8; // One edge - W
                case 11:
                case 15:
                case 43:
                case 47:
                case 139:
                case 143:
                case 171:
                case 175:
                    return 0; // 2 edges - NW
                case 22:
                case 23:
                case 54:
                case 55:
                case 150:
                case 151:
                case 182:
                case 183:
                    return 2; // 2 edges - NE
                case 208:
                case 209:
                case 212:
                case 213:
                case 240:
                case 241:
                case 244:
                case 245:
                    return 18; // 2 edges - SE
                case 104:
                case 105:
                case 108:
                case 109:
                case 232:
                case 233:
                case 236:
                case 237:
                    return 16; // 2 edges - SW
                case 66:
                case 67:
                case 70:
                case 71:
                case 98:
                case 99:
                case 102:
                case 103:
                case 194:
                case 195:
                case 198:
                case 199:
                case 226:
                case 227:
                case 230:
                case 231:
                    return 6; // 2 edges - NS (vertical)
                case 24:
                case 25:
                case 28:
                case 29:
                case 56:
                case 57:
                case 60:
                case 61:
                case 152:
                case 153:
                case 156:
                case 157:
                case 184:
                case 185:
                case 188:
                case 189:
                    return 7; // 2 edges - WE (horizontal)
                case 2:
                case 3:
                case 6:
                case 7:
                case 34:
                case 35:
                case 38:
                case 39:
                case 130:
                case 131:
                case 134:
                case 135:
                case 162:
                case 163:
                case 166:
                case 167:
                    return 4; // 3 edges - N
                case 16:
                case 17:
                case 20:
                case 21:
                case 48:
                case 49:
                case 52:
                case 53:
                case 144:
                case 145:
                case 148:
                case 149:
                case 176:
                case 177:
                case 180:
                case 181:
                    return 13; // 3 edges - E
                case 64:
                case 65:
                case 68:
                case 69:
                case 96:
                case 97:
                case 100:
                case 101:
                case 192:
                case 193:
                case 196:
                case 197:
                case 224:
                case 225:
                case 228:
                case 229:
                    return 20; // 3 edges - S
                case 8:
                case 9:
                case 12:
                case 13:
                case 40:
                case 41:
                case 44:
                case 45:
                case 136:
                case 137:
                case 140:
                case 141:
                case 168:
                case 169:
                case 172:
                case 173:
                    return 11; // 3 edges - W
                case 127:
                    return 3; // 1 corner - NW
                case 223:
                    return 5; // 1 corner - NE
                case 254:
                    return 21; // 1 corner - SE
                case 251:
                    return 19; // 1 corner - SW
                case 95:
                    return 30; // 2 corners - N
                case 222:
                    return 31; // 2 corners - E
                case 250:
                    return 39; // 2 corners - S
                case 123:
                    return 38; // 2 corners - W
                case 126:
                    return 42; // 2 corners - NW SE
                case 219:
                    return 43; // 2 corners - NE SW
                case 91:
                    return 14; // 3 corners - NW
                case 94:
                    return 15; // 3 corners - NE
                case 218:
                    return 23; // 3 corners - SE
                case 122:
                    return 22; // 3 corners - SW
                case 90:
                    return 12; // 4 corners
                case 26:
                case 58:
                case 154:
                case 186:
                    return 24; // 1 edge 2 corners - N
                case 82:
                case 83:
                case 114:
                case 115:
                    return 25; // 1 edge 2 corners - E
                case 88:
                case 89:
                case 92:
                case 93:
                    return 33; // 1 edge 2 corners - S
                case 74:
                case 78:
                case 202:
                case 206:
                    return 32; // 1 edge 2 corners - W
                case 10:
                case 14:
                case 42:
                case 46:
                case 138:
                case 142:
                case 170:
                case 174:
                    return 40; // 2 edges 1 corner - NW
                case 18:
                case 19:
                case 50:
                case 51:
                case 146:
                case 147:
                case 178:
                case 179:
                    return 41; // 2 edges 1 corner - NE
                case 80:
                case 81:
                case 84:
                case 85:
                case 112:
                case 113:
                case 116:
                case 117:
                    return 49; // 2 edges 1 corner - SE
                case 72:
                case 73:
                case 76:
                case 77:
                case 200:
                case 201:
                case 204:
                case 205:
                    return 48; // 2 edges 1 corner - SW
                case 27:
                case 59:
                case 155:
                case 187:
                    return 26; // 1 side 1 corner - N L
                case 86:
                case 87:
                case 118:
                case 119:
                    return 27; // 1 side 1 corner - E L
                case 216:
                case 217:
                case 220:
                case 221:
                    return 35; // 1 side 1 corner - S L
                case 106:
                case 110:
                case 234:
                case 238:
                    return 34; // 1 side 1 corner - W L
                case 30:
                case 62:
                case 158:
                case 190:
                    return 28; // 1 side 1 corner - N R
                case 210:
                case 211:
                case 242:
                case 243:
                    return 29; // 1 side 1 corner - E R
                case 120:
                case 121:
                case 124:
                case 125:
                    return 37; // 1 side 1 corner - S R
                case 75:
                case 79:
                case 203:
                case 207:
                    return 36; // 1 side 1 corner - W R
                case 255:
                    return 9; // 0 edges 0 corners
            }
            return 50;
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            foreach (var p in new BoundsInt(-1, -1, 0, 3, 3, 1).allPositionsWithin)
            {
                tilemap.RefreshTile(position + p);
            }
        }

        private bool TileValue(ITilemap tileMap, Vector3Int position)
        {
            TileBase tile = tileMap.GetTile(position);
            return (tile != null && tile == this);
        }

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {
            tileData.transform = Matrix4x4.identity;
            tileData.color = Color.white;
        
            textureID = 0;
            textureID = (TileValue(tileMap, location + new Vector3Int(-1, 1, 0)) == false) ? textureID + 0 : textureID + 128;
            textureID = (TileValue(tileMap, location + new Vector3Int(0, 1, 0)) == false) ? textureID + 0 : textureID + 64;
            textureID = (TileValue(tileMap, location + new Vector3Int(1, 1, 0)) == false) ? textureID + 0 : textureID + 32;
            textureID = (TileValue(tileMap, location + new Vector3Int(-1, 0, 0)) == false) ? textureID + 0 : textureID + 16;
            textureID = (TileValue(tileMap, location + new Vector3Int(1, 0, 0)) == false) ? textureID + 0 : textureID + 8;
            textureID = (TileValue(tileMap, location + new Vector3Int(-1, -1, 0)) == false) ? textureID + 0 : textureID + 4;
            textureID = (TileValue(tileMap, location + new Vector3Int(0, -1, 0)) == false) ? textureID + 0 : textureID + 2;
            textureID = (TileValue(tileMap, location + new Vector3Int(1, -1, 0)) == false) ? textureID + 0 : textureID + 1;

            if (GetIndex((byte)textureID) < spriteArray.Length)
            {
                tileData.sprite = spriteArray[GetIndex((byte)textureID)];
                tileData.color = Color.white;
                tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
                tileData.colliderType = Tile.ColliderType.Sprite;
            }
        }

        public Sprite GetPreview()
        {
            return spriteArray[GetIndex((byte)textureID)];
        }

    }
}