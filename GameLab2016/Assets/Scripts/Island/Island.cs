﻿using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using Simoncouche.Controller;

namespace Simoncouche.Islands {
    /// <summary>
    /// The global Island information, parent to Island Chunk
    /// </summary>
    public class Island : MonoBehaviour {

        private int _weight = 1;
        public int weight {
            get { return _weight; }
            protected set { _weight = value; }
        }

        /// <summary> The many part of the Island </summary>
        public List<IslandChunk> chunks { get; private set; }

        // EVENTS

        /// <summary>Invoked when this island or one of its chunks are grabbed by the player</summary>
        public PlayerGrabEvent GrabbedByPlayer;

        /// <summary>Invoked when this island or one of its chunks are released by the player</summary>
        public Rigidbody2DEvent ReleasedByPlayer;

        //Island's Components
        private CircleCollider2D _collider;
        private IslandColliders _islandColliders;
        public GravityBody gravityBody { get; private set; }
        public new Rigidbody2D rigidbody { get; private set;}

        private void Awake() {
            chunks = new List<IslandChunk>();
            _collider = GetComponent<CircleCollider2D>();
            gravityBody = GetComponent<GravityBody>();
            rigidbody = GetComponent<Rigidbody2D>();
            _islandColliders = GetComponentInChildren<IslandColliders>();

            this.GrabbedByPlayer = new PlayerGrabEvent();
            this.ReleasedByPlayer = new Rigidbody2DEvent();
        }
        
        private void Start() {
            if (_collider != null) _collider.isTrigger = true;
        }

        /// <summary>
        /// Returns if this Island has the target chunk
        /// </summary>
        /// <param name="chunk">Target chunk</param>
        /// <returns>True if it contains the chunk</returns>
        public bool IslandContainsChunk(IslandChunk chunk) {
            return chunks.Contains(chunk);
        }

        /// <summary>
        /// Add a chunk to this Island, used when a chunk collides with a Island.
        /// </summary>
        /// <param name="chunk">
        /// Reference to the collinding chunk.
        /// Raises the MergeIntoIsland event in the chunk.
        /// </param>
        public void AddChunkToIsland(IslandChunk chunk) {
            if (!chunks.Contains(chunk) && chunk!=null) {
                //_islandColliders.AddCollision(chunk);
                chunk.parentIsland = this;
                chunk.transform.SetParent(transform, true);
                chunks.Add(chunk);
                ChangeGravityBodyWhenMerging(chunk);

                chunk.MergeIntoIsland.Invoke(this);
            }
        }

        public void ConnectIslandToIsland(Vector3 targetPos, Vector3 targetRot, Island targetIsland, float time = 0.5f) {
            foreach (IslandChunk chunk in chunks) {
                foreach (IslandChunk targetChunk in targetIsland.chunks) {
                    if (chunk != null && targetChunk != null) Physics2D.IgnoreCollision(chunk.GetComponent<Collider2D>(), targetChunk.GetComponent<Collider2D>(), true);
                }
            }
            transform.DOLocalRotate(targetRot, time);
            transform.DOLocalMove(targetPos, time);
            //StartCoroutine(Delay_CenterIslandRoot(time + 0.1f, targetIsland));
        }

        /// <summary> Calls Center Island root function on a delay t in seconds </summary>
        //private IEnumerator Delay_CenterIslandRoot(float t, Island targetIsland) { yield return new WaitForSeconds(t); targetIsland.CenterIslandRoot(); }

        /// <summary>
        /// Recreate island connection for every chunk in island
        /// </summary>
        public void RecreateIslandChunkConnection() {
            foreach (IslandChunk chunk in chunks) {
                chunk.CheckConnection();
            }
            foreach (IslandChunk chunk in chunks) {
                chunk.CheckLinkConnection();
            }
        }

        /// <summary>
        /// Remove a chunk of this island.
        /// </summary>
        /// <param name="chunk">Reference of the chunk to remove</param>
        public void RemoveChunkToIsland(IslandChunk chunk) {
            //TODO : Implement this function
            if (chunks.Contains(chunk)) {
                //_islandColliders.RemoveCollision(chunk);
                chunks.Remove(chunk);
            }

            //Recenter island middle
            //CenterIslandRoot();
        }

        /// <summary>
        /// Changes the velocity of the entire Island based on new fragment
        /// </summary>
        /// <param name="chunk"></param>
        private void ChangeGravityBodyWhenMerging(IslandChunk chunk) {
            gravityBody.LinearDrag = chunk.gravityBody.LinearDrag;
            //Merge weight
            //Debug.Log(gravityBody.Velocity + "  " +  weight + "  " + chunk.gravityBody.Velocity + "  " + chunk.weight + "  result : " + (gravityBody.Velocity * weight + chunk.gravityBody.Velocity * chunk.weight) / (weight + chunk.weight));
            gravityBody.Velocity = (gravityBody.Velocity * weight + chunk.gravityBody.Velocity * chunk.weight) / (weight + chunk.weight);
            weight = weight + chunk.weight;

            _collider.radius += 0.25f; //TODO : Get Collider Position and Radius based on island chunks. This is only placeholder !
            gravityBody.Weight += chunk.gravityBody.Weight;

            //deactivate the gravitybody of the chunk
            chunk.gravityBody.DeactivateGravityBody();
        }

        /// <summary>
        /// Centers the root of the island based on its existing chunk
        /// </summary>
        public void CenterIslandRoot() {
            //Avoid error if chunks aren't properly initialized
            if (chunks.Count == 0) {
                Debug.Log("Trying to center an island without chunks.");
                return; 
            }

            //Calculate median position of the island's chunks
            Vector3 medianPosition = Vector3.zero;
            for(int i = 0; i < chunks.Count; i++ ) {
                medianPosition += chunks[i].transform.localPosition;
            }
            medianPosition /= chunks.Count;
            //Debug.Log("median position " + medianPosition);

            //Modifiy Island chunks and island positions
            for (int i = 0; i < chunks.Count; i++) {
                chunks[i].transform.localPosition -= medianPosition;
            }
            transform.position += medianPosition;

            Debug.Log("test");
            //_islandColliders.UpdateCollision();
        }
        
        /// <summary>
        /// Method called when entering the maelstrom
        /// </summary>
        /// <param name="triggerChunk"> Chunk that triggered the maelstrom enter</param>
        public void OnMaelstromEnter(IslandChunk triggerChunk) {
            //Handle Score or island destruction
            /*foreach (IslandChunk chunk in chunks) {
                PlayerGrab.UngrabBody(chunk.gravityBody);
            }*/
            RemoveChunkToIsland(triggerChunk);
            GameManager.islandManager.DestroyChunk(triggerChunk);
            if (this.chunks.Count <= 0) {
                GameManager.islandManager.DestroyIsland(this);
            }
            else {
                GameManager.islandManager.CheckIslandBroken(this);
            }
        }

        /// <summary>
        /// Changes the collision between target chunk and island
        /// </summary>
        /// <param name="?"></param>
        public void ChangeCollisionInIsland(IslandChunk t_chunk, bool nowColliding) {
            foreach (IslandChunk chunk in chunks) {
                if (chunk != t_chunk) {
                    chunk.ChangeCollisionBetweenChunk(t_chunk, nowColliding);
                }
            }
        }

        //Conversion variables
        private List<IslandChunk> _convertingChunks = new List<IslandChunk>(); //chunks to be converted if conversion timer ends
        private int _conversionAmtStatus = 0; //Store diff between chunks of both players. negative = cthulhu wins, 0 = stalemate, positive = sobek wins
        private int _amtIslandPerConversion = 1; //Amt of island per conversion
        private float _conversionTime = 5f; //Time for a conversion

        public void UpdateConversionStatus() {
            //Check which side is winning conversion
           
            List<IslandChunk> sobekChunks = new List<IslandChunk>();
            List<IslandChunk> cthulhuChunks = new List<IslandChunk>();
            foreach (IslandChunk ic in chunks) {
                if (ic.color == IslandUtils.color.red) {
                    sobekChunks.Add(ic);
                } else if (ic.color == IslandUtils.color.blue) {
                    cthulhuChunks.Add(ic);
                }
            }
            int diffIslandColors = sobekChunks.Count - cthulhuChunks.Count;
            if (diffIslandColors == 0) {        //Stalemate
                StopConversionProcess();                
            }        
            else if (diffIslandColors > 0) {    //Sobek wins
                if (_conversionAmtStatus <= 0) ResetConversionProcess(sobekChunks);
                else UpdateConvertingChunks(sobekChunks);
            } 
            else {                              //Cthulhu wins
                if (_conversionAmtStatus >= 0) ResetConversionProcess(cthulhuChunks);
                else UpdateConvertingChunks(cthulhuChunks);
            }

            //Update Conversions amount status variable
            _conversionAmtStatus = diffIslandColors;

        }

        /// <summary> Update the currently converting chunks. Used when an chunk is added to an island that is currently converting. </summary>
        /// <param name="chunks"> list of possible chunks to add. We travel this list from the last element to get the newest ones first</param>
        private void UpdateConvertingChunks(List<IslandChunk> chunks) {
            for (int i = _amtIslandPerConversion - 1; i > 0; --i) {
                if (_convertingChunks.Count >= _amtIslandPerConversion) break; //Return if list is of target size
                if(!_convertingChunks.Contains(chunks[i])) {
                    _convertingChunks.Add(chunks[i]); //Add to converting chunk
                }          
            }
        }

        /// <summary> Resets the conversion process on island. </summary>
        /// <param name="chunks"> list of chunks to select from for conversion</param>
        /// <param name="amt"> max amount of chunks to select </param>
        private void ResetConversionProcess(List<IslandChunk> chunks) {
            StopCoroutine(ConversionProcess());
            for(int i = 0; i < _amtIslandPerConversion; ++i) {
                if (chunks.Count == 0) break; //Return if list is empty
                int rIndex = Random.Range(0, chunks.Count); //Get random index
                _convertingChunks.Add(chunks[rIndex]); //Add to converting chunk
                chunks.RemoveAt(rIndex); // Remove from pool
            }
            StartCoroutine(ConversionProcess());
        }

        /// <summary> Stops the conversion process on island</summary>
        private void StopConversionProcess() {
            _convertingChunks.Clear(); //Clear the chunk list
            StopCoroutine(ConversionProcess());
        }


        private IEnumerator ConversionProcess() {
            yield return new WaitForSeconds(_conversionTime);
            IslandUtils.color newColor;
            if (_conversionAmtStatus > 0) newColor = IslandUtils.color.red;
            else if (_conversionAmtStatus < 0) newColor = IslandUtils.color.blue;
            else {
                newColor = IslandUtils.color.green;
                Debug.LogError("Tried to convert when island was equal !");
            }

            foreach(IslandChunk ic in _convertingChunks) {
                ic.ConvertChunkToAnotherColor(newColor);
            }

        }

    }
}
