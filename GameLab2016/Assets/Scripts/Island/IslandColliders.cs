using UnityEngine;
using System.Collections.Generic;

namespace Simoncouche.Islands {
    public class IslandColliders : MonoBehaviour {

        private List<IslandCollider_Data> colliders = new List<IslandCollider_Data>();
        private Transform originIsland = null;
        private Transform targetPlayer = null;
        private Vector3 originLocalPos;
        
        void Awake() {
            originIsland = transform.parent;
            originLocalPos = transform.localPosition;
        }

        /// <summary>
        /// Move the collision from the island to the grabed player
        /// </summary>
        /// <param name="destination"></param>
        public void MoveCollisionToPlayer(Transform destination) {
            targetPlayer = destination;
            transform.SetParent(destination, true);
            UpdateCollision();
        }

        /// <summary>
        /// Move the collision back to the island
        /// </summary>
        public void MoveCollisionBackToIsland() {
            targetPlayer = null;
            transform.SetParent(originIsland, true);
            transform.localPosition = originLocalPos;
            UpdateCollision();
        }

        public void AddCollision(IslandChunk chunk, Vector3 targetPos) {
            if (FindChunk(chunk) == null) {
                CircleCollider2D col = chunk.GetComponent<CircleCollider2D>();

                CircleCollider2D thisCol = gameObject.AddComponent<CircleCollider2D>();
                thisCol.radius = col.radius;
                thisCol.offset = targetPos;
                
                colliders.Add(new IslandCollider_Data(thisCol, chunk));
                Physics2D.IgnoreCollision(chunk.GetComponent<CircleCollider2D>(), thisCol);
            }
        }

        public void RemoveCollision(IslandChunk chunk) {
            IslandCollider_Data data = FindChunk(chunk);
            colliders.Remove(data);
            Destroy(data.collider);
            if (colliders.Count <= 0) {
                Destroy(gameObject);
            }
        }

        private void UpdateCollision() {
            foreach (IslandCollider_Data data in colliders) {
                data.collider.offset = data.originChunk.transform.localPosition;
            }
        }

        private IslandCollider_Data FindChunk(IslandChunk chunk) {
            foreach (IslandCollider_Data data in colliders) {
                if (data.originChunk == chunk) {
                    return data;
                }
            }
            return null;
        }

        private class IslandCollider_Data {
            public CircleCollider2D collider;
            public IslandChunk originChunk;

            public IslandCollider_Data(CircleCollider2D col, IslandChunk chunk) {
                collider = col;
                originChunk = chunk;
            }
        }
    }
}
