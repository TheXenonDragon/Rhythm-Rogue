using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfViewController : MonoBehaviour {
	/*	Fields	*/

	//
	private float animationSpeed = 0.1f;

	//	Object and script references
	public GameObject player;
	public MapManager mapManager;
	public BeatController beatController;

	//	Prefabs
	public GameObject fieldOfViewPrefab;
	public GameObject animatedFieldOfViewPrefab;

	//	View layers and Ranges
	public LayerMask layerMask;
	private float viewRange;
	
	//	Sizes
	private float tileSize;
	private int totalSize;
	private int chamberSize;
	
	//	Field of view matrix
	private GameObject[] cornerBlocks;
	private GameObject[] adjacentBlocks;

	//	Animation
	private GameObject firstAnimationHolder;
	private GameObject secondAnimationHolder;
	private List<List<GameObject>> firstAnimationBlocks;
	private List<List<GameObject>> secondAnimationBlocks;
	private List<List<Animator>> firstAnimationAnimators;
	private List<List<Animator>> secondAnimationAnimators;
	private AnimationState[] firstAnimationStates;
	private AnimationState[] secondAnimationStates;
	private bool firstAnimationIsPlaying;
	private bool secondAnimationIsPlaying;

	//	Position
	private Vector2Int currentChamberPosition;
	private Vector2Int moveDirection;
	private Vector3 firstCorridorHitPosition;
	private Vector3 secondCorridorHitPosition;

	//	Update
	private bool updateFieldOfViewPlacement;
	
	//	Enums
	private enum ChamberIndex {Up, Right, Down, Left};
	private enum AnimationState {Default, Lowered, Raised}



	/*	Methods	*/

	//	Set up
	private void Start() {
		beatController.BeatExecuted += UpdateFieldOfView;
	}
	
	public void Reset(FloorGenerator floorGenerator){
		tileSize = floorGenerator.tileSize;
		totalSize = floorGenerator.totalSize;
		chamberSize = floorGenerator.chamberSize;

		viewRange = 3 * tileSize;

		currentChamberPosition = Vector2Int.zero;
		moveDirection = Vector2Int.zero;
		firstCorridorHitPosition = Vector3.zero;
		secondCorridorHitPosition = Vector3.zero;

		//	If corner blocks and adjacent blocks do not exist, create them.
		if(cornerBlocks == null){
			CreateFieldOfView();
			CreateAnimatedBlockAndAnimators();
		}

		updateFieldOfViewPlacement = false;
		firstAnimationIsPlaying = false;
		secondAnimationIsPlaying = false;

		MoveFieldOfView();
		EnableAllAdjacentBlocks();
	}

	private void CreateFieldOfView(){
		//	Create the 8 objects to block view.  Size according to chamber size.
		cornerBlocks = new GameObject[4];
		adjacentBlocks = new GameObject[4];

		//	Instantiate and scale Corner blocks.
		for(int i = 0; i < cornerBlocks.Length; i++){
			cornerBlocks[i] = Instantiate<GameObject>(fieldOfViewPrefab, Vector3.one * -100, fieldOfViewPrefab.transform.rotation, transform);
			cornerBlocks[i].transform.localScale = new Vector3(chamberSize * tileSize, cornerBlocks[i].transform.localScale.y, chamberSize * tileSize);
			cornerBlocks[i].name = "Corner Block #" + i;
		}

		//	Instantiate and scale Adjacent blocks.
		for(int i = 0; i < adjacentBlocks.Length; i++){
			adjacentBlocks[i] = Instantiate<GameObject>(fieldOfViewPrefab, Vector3.zero, fieldOfViewPrefab.transform.rotation, transform);
			adjacentBlocks[i].transform.localScale = new Vector3(chamberSize * tileSize, adjacentBlocks[i].transform.localScale.y, chamberSize * tileSize);
			adjacentBlocks[i].name = "Adjacent Block #" + i;
		}
	}

	private void CreateAnimatedBlockAndAnimators(){
		//	FIRST ANIMATION BLOCK	//
		
		firstAnimationHolder = new GameObject("First Animation Block Holder");
		firstAnimationHolder.transform.parent = transform;
		firstAnimationHolder.SetActive(false);

		firstAnimationBlocks = new List<List<GameObject>>();
		firstAnimationAnimators = new List<List<Animator>>();

		//	Create center block
		firstAnimationBlocks.Add(new List<GameObject>());
		firstAnimationBlocks[0].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, Vector3.zero, animatedFieldOfViewPrefab.transform.rotation));
		firstAnimationBlocks[0][0].transform.parent = firstAnimationHolder.transform;
		firstAnimationBlocks[0][0].transform.localScale = new Vector3(2 * tileSize, animatedFieldOfViewPrefab.transform.localScale.y, 2 * tileSize);

		//	Create all other blocks
		for(int i = 1; i < (chamberSize / 2); i++){
			firstAnimationBlocks.Add(new List<GameObject>());

			//	Create Edges
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.forward * 1.5f * tileSize) + (Vector3.forward * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.right * 1.5f * tileSize) + (Vector3.right * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.left * 1.5f * tileSize) + (Vector3.left * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.back * 1.5f * tileSize) + (Vector3.back * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));

			//	Create Corners
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.back * 1.5f * tileSize) + (Vector3.back * (i - 1) * tileSize) + (Vector3.right * 1.5f * tileSize) + (Vector3.right * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.back * 1.5f * tileSize) + (Vector3.back * (i - 1) * tileSize) + (Vector3.left * 1.5f * tileSize) + (Vector3.left * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.forward * 1.5f * tileSize) + (Vector3.forward * (i - 1) * tileSize) + (Vector3.right * 1.5f * tileSize) + (Vector3.right * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			firstAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.forward * 1.5f * tileSize) + (Vector3.forward * (i - 1) * tileSize) + (Vector3.left * 1.5f * tileSize) + (Vector3.left * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));

			//	Parenting
			firstAnimationBlocks[i][0].transform.parent = firstAnimationHolder.transform;	//	Edge
			firstAnimationBlocks[i][1].transform.parent = firstAnimationHolder.transform;	//	Edge
			firstAnimationBlocks[i][2].transform.parent = firstAnimationHolder.transform;	//	Edge
			firstAnimationBlocks[i][3].transform.parent = firstAnimationHolder.transform;	//	Edge
			firstAnimationBlocks[i][4].transform.parent = firstAnimationHolder.transform;	//	Corner
			firstAnimationBlocks[i][5].transform.parent = firstAnimationHolder.transform;	//	Corner
			firstAnimationBlocks[i][6].transform.parent = firstAnimationHolder.transform;	//	Corner
			firstAnimationBlocks[i][7].transform.parent = firstAnimationHolder.transform;	//	Corner

			//	Scale
			firstAnimationBlocks[i][0].transform.localScale = new Vector3((i * 2 * tileSize), animatedFieldOfViewPrefab.transform.localScale.y, tileSize);	//	Edge
			firstAnimationBlocks[i][1].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, (i * 2 * tileSize));	//	Edge
			firstAnimationBlocks[i][2].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, (i * 2 * tileSize));	//	Edge
			firstAnimationBlocks[i][3].transform.localScale = new Vector3((i * 2 * tileSize), animatedFieldOfViewPrefab.transform.localScale.y, tileSize);	//	Edge
			firstAnimationBlocks[i][4].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
			firstAnimationBlocks[i][5].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
			firstAnimationBlocks[i][6].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
			firstAnimationBlocks[i][7].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
		}

		//	Add animators
		for(int i = 0; i < firstAnimationBlocks.Count; i++){
			firstAnimationAnimators.Add(new List<Animator>());
			for(int j = 0; j < firstAnimationBlocks[i].Count; j++){
				firstAnimationAnimators[i].Add(firstAnimationBlocks[i][j].transform.GetChild(0).GetComponent<Animator>());
			}
		}

		//	Add animation state
		firstAnimationStates = new AnimationState[firstAnimationBlocks.Count];


		//	SECOND ANIMATION BLOCK	//

		secondAnimationHolder = new GameObject("Second Animation Block Holder");
		secondAnimationHolder.transform.parent = transform;
		secondAnimationHolder.SetActive(false);

		secondAnimationBlocks = new List<List<GameObject>>();
		secondAnimationAnimators = new List<List<Animator>>();

		//	Create center block
		secondAnimationBlocks.Add(new List<GameObject>());
		secondAnimationBlocks[0].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, Vector3.zero, animatedFieldOfViewPrefab.transform.rotation));
		secondAnimationBlocks[0][0].transform.parent = secondAnimationHolder.transform;
		secondAnimationBlocks[0][0].transform.localScale = new Vector3(2 * tileSize, animatedFieldOfViewPrefab.transform.localScale.y, 2 * tileSize);

		//	Create all other blocks
		for(int i = 1; i < (chamberSize / 2); i++){
			secondAnimationBlocks.Add(new List<GameObject>());

			//	Create Edges
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.forward * 1.5f * tileSize) + (Vector3.forward * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.right * 1.5f * tileSize) + (Vector3.right * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.left * 1.5f * tileSize) + (Vector3.left * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.back * 1.5f * tileSize) + (Vector3.back * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));

			//	Create Corners
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.back * 1.5f * tileSize) + (Vector3.back * (i - 1) * tileSize) + (Vector3.right * 1.5f * tileSize) + (Vector3.right * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.back * 1.5f * tileSize) + (Vector3.back * (i - 1) * tileSize) + (Vector3.left * 1.5f * tileSize) + (Vector3.left * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.forward * 1.5f * tileSize) + (Vector3.forward * (i - 1) * tileSize) + (Vector3.right * 1.5f * tileSize) + (Vector3.right * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));
			secondAnimationBlocks[i].Add(Instantiate<GameObject>(animatedFieldOfViewPrefab, (Vector3.forward * 1.5f * tileSize) + (Vector3.forward * (i - 1) * tileSize) + (Vector3.left * 1.5f * tileSize) + (Vector3.left * (i - 1) * tileSize), animatedFieldOfViewPrefab.transform.rotation));

			//	Parenting
			secondAnimationBlocks[i][0].transform.parent = secondAnimationHolder.transform;	//	Edge
			secondAnimationBlocks[i][1].transform.parent = secondAnimationHolder.transform;	//	Edge
			secondAnimationBlocks[i][2].transform.parent = secondAnimationHolder.transform;	//	Edge
			secondAnimationBlocks[i][3].transform.parent = secondAnimationHolder.transform;	//	Edge
			secondAnimationBlocks[i][4].transform.parent = secondAnimationHolder.transform;	//	Corner
			secondAnimationBlocks[i][5].transform.parent = secondAnimationHolder.transform;	//	Corner
			secondAnimationBlocks[i][6].transform.parent = secondAnimationHolder.transform;	//	Corner
			secondAnimationBlocks[i][7].transform.parent = secondAnimationHolder.transform;	//	Corner

			//	Scale
			secondAnimationBlocks[i][0].transform.localScale = new Vector3((i * 2 * tileSize), animatedFieldOfViewPrefab.transform.localScale.y, tileSize);	//	Edge
			secondAnimationBlocks[i][1].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, (i * 2 * tileSize));	//	Edge
			secondAnimationBlocks[i][2].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, (i * 2 * tileSize));	//	Edge
			secondAnimationBlocks[i][3].transform.localScale = new Vector3((i * 2 * tileSize), animatedFieldOfViewPrefab.transform.localScale.y, tileSize);	//	Edge
			secondAnimationBlocks[i][4].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
			secondAnimationBlocks[i][5].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
			secondAnimationBlocks[i][6].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
			secondAnimationBlocks[i][7].transform.localScale = new Vector3(tileSize, animatedFieldOfViewPrefab.transform.localScale.y, tileSize);				//	Corner
		}

		//	Add animators
		for(int i = 0; i < secondAnimationBlocks.Count; i++){
			secondAnimationAnimators.Add(new List<Animator>());
			for(int j = 0; j < secondAnimationBlocks[i].Count; j++){
				secondAnimationAnimators[i].Add(secondAnimationBlocks[i][j].transform.GetChild(0).GetComponent<Animator>());
			}
		}

		//	Add animation state
		secondAnimationStates = new AnimationState[secondAnimationBlocks.Count];
	}


	//	Beat Controller Caller
	private void UpdateFieldOfView(object sender, EventArgs e){
		updateFieldOfViewPlacement = true;
	}

	
	//	Update chamber placement
	private void Update() {
		UpdateFieldOfView();
	}

	private void UpdateFieldOfView(){
		//	This method calls all other update related methods.  
		//	It can only be called once per beat.
		//	The reason this is not called during the beat is to relieve computational stress.

		if(updateFieldOfViewPlacement){
			MoveFieldOfView();
			CheckAndHideTrailingBlock();
			CheckAndRevealArea();

			updateFieldOfViewPlacement = false;
		}
	}

	private void UpdateCurrentChamberIndex(){
		//	Measure distance between player position and middle of each chamber.  Closest chamber is the current chamber.
		int xOffset = chamberSize * currentChamberPosition.x;
		int yOffset = chamberSize * currentChamberPosition.y;
		Vector2Int currentPlayerPositionInChamber = new Vector2Int(mapManager.GetPlayerPosition().x - xOffset, mapManager.GetPlayerPosition().y - yOffset);

		if(currentPlayerPositionInChamber.x > chamberSize - 1){
			//	Player went to the chamber to the right.
			currentChamberPosition = new Vector2Int(currentChamberPosition.x + 1, currentChamberPosition.y);
			moveDirection = Vector2Int.right;
		}
		else if(currentPlayerPositionInChamber.x < 0){
			//	Player went to the chamber to the left.
			currentChamberPosition = new Vector2Int(currentChamberPosition.x - 1, currentChamberPosition.y);
			moveDirection = Vector2Int.left;
		}
		else if(currentPlayerPositionInChamber.y > chamberSize - 1){
			//	Player went to the chamber above.
			currentChamberPosition = new Vector2Int(currentChamberPosition.x, currentChamberPosition.y + 1);
			moveDirection = Vector2Int.up;
		}
		else if(currentPlayerPositionInChamber.y < 0){
			//	Player went to the chamber below.
			currentChamberPosition = new Vector2Int(currentChamberPosition.x, currentChamberPosition.y - 1);
			moveDirection = Vector2Int.down;
		}

	}

	private void MoveFieldOfView(){
		//	If player enters a new chamber, move the field of view.
		int chamberSizeOffset = chamberSize / 2;
		float overlapOffset = 0.001f;

		//	Check if the chamber index has changed.
		UpdateCurrentChamberIndex();


		//	Set the partial positions.
		float topPostion = (((currentChamberPosition.y + 1) * chamberSize) + chamberSizeOffset) * tileSize - (tileSize / 2);
		float rightPosition = (((currentChamberPosition.x + 1) * chamberSize) + chamberSizeOffset) * tileSize - (tileSize / 2);
		float bottomPosition = (((currentChamberPosition.y) * chamberSize) - chamberSizeOffset) * tileSize - (tileSize / 2);
		float leftPosition = (((currentChamberPosition.x) * chamberSize) - chamberSizeOffset) * tileSize - (tileSize / 2);

		float xMiddlePosition = (((currentChamberPosition.x) * chamberSize) + chamberSizeOffset) * tileSize - (tileSize / 2);
		float yMiddlePosition = (((currentChamberPosition.y) * chamberSize) + chamberSizeOffset) * tileSize - (tileSize / 2);


		//	Top Edge Block Position
		adjacentBlocks[(int)ChamberIndex.Up].transform.position = new Vector3(xMiddlePosition, overlapOffset, topPostion + overlapOffset);
		
		//	Right Edge Block Position
		adjacentBlocks[(int)ChamberIndex.Right].transform.position = new Vector3(rightPosition + overlapOffset, overlapOffset, yMiddlePosition);
		
		//	Bottom Edge Block Position
		adjacentBlocks[(int)ChamberIndex.Down].transform.position = new Vector3(xMiddlePosition, overlapOffset, bottomPosition - overlapOffset);
		
		//	Left Edge Block Position
		adjacentBlocks[(int)ChamberIndex.Left].transform.position = new Vector3(leftPosition - overlapOffset, overlapOffset, yMiddlePosition);
		

		//	Top - Right Corner Block Position
		cornerBlocks[0].transform.position = new Vector3(rightPosition, overlapOffset, topPostion);

		//	Bottom - Right Corner Block Position
		cornerBlocks[1].transform.position = new Vector3(rightPosition, overlapOffset, bottomPosition);

		//	Bottom - Left Corner Block Position
		cornerBlocks[2].transform.position = new Vector3(leftPosition, overlapOffset, bottomPosition);

		//	Top - Left Corner Block Position
		cornerBlocks[3].transform.position = new Vector3(leftPosition, overlapOffset, topPostion);
	}
	

	//	Hide Blocks
	private void CheckAndHideTrailingBlock(){
		//	If the player passess from one chamber to another, hide the trailing chamber.
		
		//	If moving down, hide the top chamber.
		if(moveDirection == Vector2Int.down){
			EnableAdjacentBlock((int)ChamberIndex.Up, false);
		}

		//	If moving left, hide the right chamber.
		if(moveDirection == Vector2Int.left){
			EnableAdjacentBlock((int)ChamberIndex.Right, false);
		}

		//	If moving up, hide the bottom chamber.
		if(moveDirection == Vector2Int.up){
			EnableAdjacentBlock((int)ChamberIndex.Down, false);
		}

		//	If moving right, hide the left chamber.
		if(moveDirection == Vector2Int.right){
			EnableAdjacentBlock((int)ChamberIndex.Left, false);
		}
	}

	private IEnumerator HideArea(int index, GameObject animationHolder, List<List<Animator>> animators){
		//	Adds the block which the player is entering into ahead of the player.

		//	The index for the area to hide.
		int trailingChamberIndexToHide = -1;
		int headingChamberIndexToHide = -1;

		//	Cache move direction to check once animation exits.
		Vector2Int cachedMoveDirection;

		//	Determine which hit position to use.
		Vector3 corridorHitPosition = (animationHolder.Equals(firstAnimationHolder)) ? firstCorridorHitPosition : secondCorridorHitPosition;

		//	Delay until the player is far enough away from the position the raycast hit the field of view in the corridor.
		while(true){
			if(Vector3.Distance(player.transform.position, corridorHitPosition) > (viewRange * 2f)){
				break;
			}

			yield return new WaitForSeconds(0.5f);
		}

		//	Animate the field of view rising. If the player changes chunks, repeat the loop.		
		while(true){
			//	Determine which block to hide.
			if (moveDirection == Vector2Int.up){
				//	Heading block index is 0 (up)
				headingChamberIndexToHide = (int)ChamberIndex.Up;
				EnableAdjacentBlock((int)ChamberIndex.Up, true);
				
				//	Trailing block index is 2 (down)
				trailingChamberIndexToHide = (int)ChamberIndex.Down;
			}
			else if (moveDirection == Vector2Int.right){
				//	Heading block index is 1 (right)
				headingChamberIndexToHide = (int)ChamberIndex.Right;
				EnableAdjacentBlock((int)ChamberIndex.Right, true);

				//	Trailing block index is 3 (left)
				trailingChamberIndexToHide = (int)ChamberIndex.Left;
			}
			else if (moveDirection == Vector2Int.down){
				//	Heading block index is 2 (down)
				headingChamberIndexToHide = (int)ChamberIndex.Down;
				EnableAdjacentBlock((int)ChamberIndex.Down, true);

				//	Trailing block index is 0 (up)
				trailingChamberIndexToHide = (int)ChamberIndex.Up;
			}
			else if (moveDirection == Vector2Int.left){
				//	Heading block index is 3 (left)
				headingChamberIndexToHide = (int)ChamberIndex.Left;
				EnableAdjacentBlock((int)ChamberIndex.Left, true);

				//	Trailing block index is 1 (right)
				trailingChamberIndexToHide = (int)ChamberIndex.Right;
			}
			else{
				EnableAdjacentBlock(index, true);
				trailingChamberIndexToHide = index;
				headingChamberIndexToHide = index;
			}

			//	Cache move direction for checking later.
			cachedMoveDirection = moveDirection;

			//	Animate rising field of view.  Wait to proceed until animation is done.
			yield return StartCoroutine(HideAreaAnimation(trailingChamberIndexToHide, cachedMoveDirection, animationHolder, animators));

			//	If move direction has changed during the animation, repeat the loop.
			if(cachedMoveDirection == moveDirection){
				break;
			}
			else{
				yield return StartCoroutine(ResetAnimationToRaise(animationHolder, animators));
			}
		}

		//	Hide the heading and trailing areas and set the moveDirection to default.
		EnableAllAdjacentBlocks();
		moveDirection = Vector2Int.zero;
		animationHolder.SetActive(false);

		if(animationHolder.Equals(firstAnimationHolder)){
			firstAnimationIsPlaying = false;
		}
		else{
			secondAnimationIsPlaying = false;
		}
	}
	
	private IEnumerator HideAreaAnimation(int index, Vector2Int moveDirectionTest, GameObject animationHolder, List<List<Animator>> animators){
		animationHolder.transform.position = adjacentBlocks[index].transform.position + (Vector3.up * 0.01f);

		//	Set animation states
		AnimationState[] animationStates = (animationHolder.Equals(firstAnimationHolder)) ? firstAnimationStates : secondAnimationStates;
		
		//	Animate
		for(int i = 0; i < animators.Count; i++){
			if(animationStates[i] != AnimationState.Raised){
				animationStates[i] = AnimationState.Raised;

				for(int j = 0; j < animators[i].Count; j++){
					animators[i][j].ResetTrigger("Raise");
					animators[i][j].SetTrigger("Raise");
				}

				//	Delay between cocentrice groups
				yield return new WaitForSeconds(animationSpeed);
			}
			
			//	If the moveDirection has changed, exit the animation.
			if(moveDirectionTest != moveDirection){
				break;
			}
		}

		//	If the moveDirection has changed, ignore the following code.
		if(moveDirectionTest == moveDirection){
			//	Delay for the clip duration so that it can finish before being reset.
			yield return new WaitForSeconds(animators[0][0].GetCurrentAnimatorClipInfo(0).Length);
		}

		//	Set the main animation state to the modified version.
		if(animationHolder.Equals(firstAnimationHolder)){
			firstAnimationStates = animationStates;
		}
		else{
			secondAnimationStates = animationStates;
		}
	}

	private IEnumerator ResetAnimationToRaise(GameObject animationHolder, List<List<Animator>> animators){
		//	Set animation states
		AnimationState[] animationStates = (animationHolder.Equals(firstAnimationHolder)) ? firstAnimationStates : secondAnimationStates;
		
		//	Animate the field of view lowering.
		for(int i = animators.Count - 1; i >= 0; i--){
			//	Check if next set of animation blocks is already lowered.
			
			if(animationStates[i] != AnimationState.Lowered){
				animationStates[i] = AnimationState.Lowered;

					for(int j = 0; j < animators[i].Count; j++){
					animators[i][j].ResetTrigger("Lower");
					animators[i][j].SetTrigger("Lower");
				}
				yield return new WaitForSeconds(animationSpeed);
			}
		}

		//	Set the main animation state to the modified version.
		if(animationHolder.Equals(firstAnimationHolder)){
			firstAnimationStates = animationStates;
		}
		else{
			secondAnimationStates = animationStates;
		}
	}


	//	Reveal Blocks
	private void CheckAndRevealArea(){
		//	If raycast hits field of view, reveal area.
		RaycastHit hit;
		int chamberIndex = -1;

		//	Up
		if(Physics.Raycast(player.transform.position, Vector3.forward, out hit, viewRange, layerMask)){
			if(hit.transform.gameObject.layer == LayerMask.NameToLayer("FieldOfView")){
				chamberIndex = (int)ChamberIndex.Up;
				
				if(!firstAnimationIsPlaying){
					firstCorridorHitPosition = hit.point;
				}
				else{
					secondCorridorHitPosition = hit.point;
				}
			}
		}

		//	Right
		if(Physics.Raycast(player.transform.position, Vector3.right, out hit, viewRange, layerMask)){
			if(hit.transform.gameObject.layer == LayerMask.NameToLayer("FieldOfView")){
				chamberIndex = (int)ChamberIndex.Right;

				if(!firstAnimationIsPlaying){
					firstCorridorHitPosition = hit.point;
				}
				else{
					secondCorridorHitPosition = hit.point;
				}
			}
		}

		//	Down
		if(Physics.Raycast(player.transform.position, Vector3.back, out hit, viewRange, layerMask)){
			if(hit.transform.gameObject.layer == LayerMask.NameToLayer("FieldOfView")){
				chamberIndex = (int)ChamberIndex.Down;

				if(!firstAnimationIsPlaying){
					firstCorridorHitPosition = hit.point;
				}
				else{
					secondCorridorHitPosition = hit.point;
				}
			}
		}

		//	Left
		if(Physics.Raycast(player.transform.position, Vector3.left, out hit, viewRange, layerMask)){
			if(hit.transform.gameObject.layer == LayerMask.NameToLayer("FieldOfView")){
				chamberIndex = (int)ChamberIndex.Left;

				if(!firstAnimationIsPlaying){
					firstCorridorHitPosition = hit.point;
				}
				else{
					secondCorridorHitPosition = hit.point;
				}
			}
		}

		//	If hit occured, hide hit block and start animation to reveal area.
		if(chamberIndex != -1){
			EnableAdjacentBlock(chamberIndex, false);

			if(!firstAnimationIsPlaying){
				firstAnimationIsPlaying = true;
				StartCoroutine(RevealAreaAnimation(chamberIndex, firstAnimationHolder, firstAnimationAnimators));
			}
			else if(!secondAnimationIsPlaying){
				secondAnimationIsPlaying = true;
				StartCoroutine(RevealAreaAnimation(chamberIndex, secondAnimationHolder, secondAnimationAnimators));
			}
		}
	}

	private IEnumerator RevealAreaAnimation(int index, GameObject animationHolder, List<List<Animator>> animators){
		//	This method animates the field of view lowering.

		//	Set animation states
		AnimationState[] animationStates = (animationHolder.Equals(firstAnimationHolder)) ? firstAnimationStates : secondAnimationStates;

		//	Set the position of the field of view and make it visible.
		animationHolder.transform.position = adjacentBlocks[index].transform.position + (Vector3.up * 0.01f);
		animationHolder.SetActive(true);

		//	Animate the field of view lowering.
		for(int i = animators.Count - 1; i >= 0; i--){
			for(int j = 0; j < animators[i].Count; j++){
				animators[i][j].ResetTrigger("Lower");
				animators[i][j].SetTrigger("Lower");
			}

			animationStates[i] = AnimationState.Lowered;

			yield return new WaitForSeconds(animationSpeed);
		}

		//	Delay for the clip duration so that it can finish before being reset.
		yield return new WaitForSeconds(animators[0][0].GetCurrentAnimatorClipInfo(0).Length);

		//	Set the main animation state to the modified version.
		if(animationHolder.Equals(firstAnimationHolder)){
			firstAnimationStates = animationStates;
		}
		else{
			secondAnimationStates = animationStates;
		}

		//	Hide revealed block
		StartCoroutine(HideArea(index, animationHolder, animators));
	}


	//	Enable or Disable mesh and box collider of blocks
	private void EnableAllAdjacentBlocks(){
		for(int i = 0; i < adjacentBlocks.Length; i++){
			adjacentBlocks[i].GetComponent<MeshRenderer>().enabled = true;
			adjacentBlocks[i].GetComponent<BoxCollider>().enabled = true;
		}
	}

	private void EnableAdjacentBlock(int index, bool revealBlock){
		adjacentBlocks[index].GetComponent<MeshRenderer>().enabled = revealBlock;
		adjacentBlocks[index].GetComponent<BoxCollider>().enabled = revealBlock;
	}
}