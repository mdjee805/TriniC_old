﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ConnectFour
{
	public class GameController : MonoBehaviour 
	{
		public GameObject popupWindow;
		public GameObject tutorialWindow;

        public void NoTutorial()
        {
			popupWindow.SetActive(false);
			StartGame();
        }
		public void YesTutorial()
		{
			popupWindow.SetActive(false);
			tutorialWindow.SetActive(true);
			
		}
        public void closeTutorial()
        {
			popupWindow.SetActive(false);
			tutorialWindow.SetActive(false);
			StartGame();
		}
		enum Piece
		{
			Empty = 0,
			Blue = 1,
			Red = 2
		}

		[Range(3, 8)]
		public int numRows = 6;
		[Range(3, 8)]
		public int numColumns = 7;

		[Tooltip("How many pieces have to be connected to win.")]
		public int numPiecesToWin = 4;

		[Tooltip("Allow diagonally connected Pieces?")]
		public bool allowDiagonally = true;
		
		public float dropTime = 4f;

		// Gameobjects 
		public GameObject pieceRed;
		public GameObject pieceBlue;
		public GameObject pieceField;

		public GameObject winningText;
		public string playerWonText = "Player 2 Wins!";
		public string playerLoseText = "Player 1 Wins!";
		public string drawText = "Draw!";

		public GameObject turnText;
		public string playerOneTurn = "Player 1's Turn";
		public string playerTwoTurn = "Player 2's Turn";
		public string endTurn = "End Game";

		public GameObject backButton;
		public GameObject resetButton;

		public GameObject backText;
		public GameObject resetText;

		public GameObject btnPlayAgain;
		bool btnPlayAgainTouching = false;
		Color btnPlayAgainOrigColor;
		Color btnPlayAgainHoverColor = new Color(255, 143,4);

		GameObject gameObjectField;

		// temporary gameobject, holds the piece at mouse position until the mouse has clicked
		GameObject gameObjectTurn;

		GameObject pieceAfterShadow;

		public Color startcolor;

		/// <summary>
		/// The Game field.
		/// 0 = Empty
		/// 1 = Blue
		/// 2 = Red
		/// </summary>
		int[,] field;

		bool isPlayersTurn = true;
		bool isLoading = true;
		bool isDropping = false; 
		bool mouseButtonPressed = false;

		bool gameOver = false;
		bool isCheckingForWinner = false;

		// Use this for initialization
		void StartGame () 
		{
			int max = Mathf.Max (numRows, numColumns);

			if(numPiecesToWin > max)
				numPiecesToWin = max;

			CreateField ();

			isPlayersTurn = System.Convert.ToBoolean(Random.Range (0, 1));
			turnText.GetComponent<TextMesh>().text = isPlayersTurn ? playerTwoTurn : playerOneTurn;
			btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;
		}

		/// <summary>
		/// Creates the field.
		/// </summary>
		void CreateField()
		{
			winningText.SetActive(false);
			btnPlayAgain.SetActive(false);
			backText.SetActive(false);
			resetText.SetActive(false);

			turnText.SetActive(true);
			backButton.SetActive(true);
			resetButton.SetActive(true);

			isLoading = true;

			gameObjectField = GameObject.Find ("Field");
			if(gameObjectField != null)
			{
				DestroyImmediate(gameObjectField);
			}
			gameObjectField = new GameObject("Field");

			// create an empty field and instantiate the cells
			field = new int[numColumns, numRows];
			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					field[x, y] = (int)Piece.Empty;
					GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
					g.transform.parent = gameObjectField.transform;
				}
			}

			isLoading = false;
			gameOver = false;

			// center camera
			Camera.main.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f), Camera.main.transform.position.z);

			winningText.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) + 1, winningText.transform.position.z);

			btnPlayAgain.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) - 1, btnPlayAgain.transform.position.z);
			
			turnText.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -6, -8);

			// backButton.transform.position = new Vector3(
			// 	1500, 900, -8);
			
			// resetButton.transform.position = new Vector3(
			// 	1500, 800, -8);

			// backText.transform.position = new Vector3(
			// 	1500, 850, -8);

			// resetText.transform.position = new Vector3(
			// 	1500, 750, -8);
		}

		/// <summary>
		/// Spawns a piece at mouse position above the first row
		/// </summary>
		/// <returns>The piece.</returns>
		GameObject SpawnPiece()
		{
			Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					
			if(!isPlayersTurn)
			{
				List<int> moves = GetPossibleMoves();

				if(moves.Count > 0)
				{
					int column = moves[Random.Range (0, moves.Count)];

					spawnPos = new Vector3(column, 0, 0);
				}
			}

			GameObject g = Instantiate(
					isPlayersTurn ? pieceBlue : pieceRed, // is players turn = spawn blue, else spawn red
					new Vector3(
					Mathf.Clamp(spawnPos.x, 0, numColumns-1), 
					gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
					Quaternion.identity) as GameObject;

			return g;
		}

		void UpdatePlayAgainButton()
		{
			RaycastHit hit;
			//ray shooting out of the camera from where the mouse is
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			
			if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					btnPlayAgainTouching = true;
					
					//CreateField();
					Application.LoadLevel(1);
				}
			}
			else
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}
			
			if(Input.touchCount == 0)
			{
				btnPlayAgainTouching = false;
			}
		}
		
		// static function NameContains(start: String, transf: Transform) {
		// 	if (transf.name.StartsWith(start)){
		// 		// this object starts with the string passed in "start":
		// 		// do whatever you want with it...
		// 		print(transf.name); // like printing its name
		// 	}
		// 	// now search in its children, grandchildren etc.
		// 	for (var child in transf){
		// 		NameContains(start, child);
		// 	}
		// }

		public void Back()
		{
			SceneManager.LoadScene(0);
		}

		void Reset()
		{
			// StartGame();
			SceneManager.LoadScene(1);
			// gameOver = true;
			// // CreateField();
			// gameObjectField = GameObject.Find ("Field");
			// DestroyImmediate(gameObjectField);
			// // gameObjectField = GameObject.Find ("Field");
			// DestroyImmediate(gameObjectTurn);
			// DestroyImmediate(pieceAfterShadow);
			// CreateField();
			// // create an empty field and instantiate the cells
			// field = new int[numColumns, numRows];
			// for(int x = 0; x < numColumns; x++)
			// {
			// 	for(int y = 0; y < numRows; y++)
			// 	{
			// 		field[x, y] = (int)Piece.Empty;
			// 		GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
			// 		g.transform.parent = gameObjectField.transform;
			// 	}
			// }
			// for(int i = numRows-1; i >= 0; i--)
			// {
			// 	if(field[x, i] == 0)
			// 	{
			// 		foundFreeCell = true;
			// 		field[x, i] = isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red;
			// 		endPosition = new Vector3(x, i * -1, startPosition.z);

			// 		break;
			// 	}
			// }
		}

		// Update is called once per frame
		void Update () 
		{
            if (isLoading)
				return;

			if(isCheckingForWinner)
				return;

			if(gameOver)
			{
				winningText.SetActive(true);
				btnPlayAgain.SetActive(true);

				UpdatePlayAgainButton();

				return;
			}

			if(gameObjectTurn == null)
			{
				gameObjectTurn = SpawnPiece();
				pieceAfterShadow = SpawnPiece();
			}
			else
			{
				// update the objects position
				Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				gameObjectTurn.transform.position = new Vector3(
					Mathf.Clamp(pos.x, 0, numColumns-1), 
					gameObjectField.transform.position.y + 1, 0);
				
				//AFTERSHADOW START
				Vector3 startPosition = gameObjectTurn.transform.position;
				Vector3 endPosition = new Vector3();

				// round to a grid cell
				int x = Mathf.RoundToInt(startPosition.x);
				startPosition = new Vector3(x, startPosition.y, startPosition.z);

				// is there a free cell in the selected column?
				bool foundFreeCell = false;
				for(int i = numRows-1; i >= 0; i--)
				{
					if(field[x, i] == 0)
					{
						foundFreeCell = true;
						// field[x, i] = isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red;
						endPosition = new Vector3(x, i * -1, startPosition.z);

						break;
					}
				}

				if(foundFreeCell)
				{
					//Creates a new after shadow piece
					pieceAfterShadow.GetComponent<Renderer>().enabled = true;
					
					//change the opacity of the game object => could possible initialize this somewhere else
					Color oldCol = gameObjectTurn.GetComponent<Renderer>().material.color;
					Color newCol = new Color(oldCol.r, oldCol.g, oldCol.b, 0.2f);
					pieceAfterShadow.GetComponent<Renderer>().material.color = newCol;
					float distance = Vector3.Distance(startPosition, endPosition);

					//this puts the shadow in the position
					pieceAfterShadow.transform.position = Vector3.Lerp (startPosition, endPosition, ((numRows - distance) + 1));
				}

				//AFTERSHADOW END

				// click the left mouse button to drop the piece into the selected column
				if(Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
				{
					mouseButtonPressed= true;

					StartCoroutine(dropPiece(gameObjectTurn));
				}
				else
				{
					mouseButtonPressed = false;
				}
			}
			
		}

		/// <summary>
		/// Gets all the possible moves.
		/// </summary>
		/// <returns>The possible moves.</returns>
		public List<int> GetPossibleMoves()
		{
			List<int> possibleMoves = new List<int>();
			for (int x = 0; x < numColumns; x++)
			{
				for(int y = numRows - 1; y >= 0; y--)
				{
					if(field[x, y] == (int)Piece.Empty)
					{
						possibleMoves.Add(x);
						break;
					}
				}
			}
			return possibleMoves;
		}

		/// <summary>
		/// This method searches for a empty cell and lets 
		/// the object fall down into this cell
		/// </summary>
		/// <param name="gObject">Game Object.</param>
		IEnumerator dropPiece(GameObject gObject)
		{
			

			isDropping = true;

			Vector3 startPosition = gObject.transform.position;
			Vector3 endPosition = new Vector3();

			// round to a grid cell
			int x = Mathf.RoundToInt(startPosition.x);
			startPosition = new Vector3(x, startPosition.y, startPosition.z);

			// is there a free cell in the selected column?
			bool foundFreeCell = false;
			for(int i = numRows-1; i >= 0; i--)
			{
				if(field[x, i] == 0)
				{
					foundFreeCell = true;
					field[x, i] = isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red;
					endPosition = new Vector3(x, i * -1, startPosition.z);

					break;
				}
			}

			if(foundFreeCell)
			{
				// Instantiate a new Piece, disable the temporary
				GameObject g = Instantiate (gObject) as GameObject;
				gameObjectTurn.GetComponent<Renderer>().enabled = false;

				float distance = Vector3.Distance(startPosition, endPosition);

				float t = 0;
				while(t < 1)
				{
					t += Time.deltaTime * dropTime * ((numRows - distance) + 1);

					g.transform.position = Vector3.Lerp (startPosition, endPosition, t);
					yield return null;
				}

				g.transform.parent = gameObjectField.transform;

				// remove the temporary gameobject
				DestroyImmediate(gameObjectTurn);
				DestroyImmediate(pieceAfterShadow);
				// run coroutine to check if someone has won
				StartCoroutine(Won());

				// wait until winning check is done
				while(isCheckingForWinner)
					yield return null;

				isPlayersTurn = !isPlayersTurn;
				turnText.GetComponent<TextMesh>().text = isPlayersTurn ? playerTwoTurn : playerOneTurn;
			}

			isDropping = false;

			yield return 0;
		}

		/// <summary>
		/// Check for Winner
		/// </summary>
		IEnumerator Won()
		{
			isCheckingForWinner = true;

			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					// Get the Laymask to Raycast against, if its Players turn only include
					// Layermask Blue otherwise Layermask Red
					int layermask = isPlayersTurn ? (1 << 8) : (1 << 9);

					// If its Players turn ignore red as Starting piece and wise versa
					if(field[x, y] != (isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red))
					{
						continue;
					}

					// shoot a ray of length 'numPiecesToWin - 1' to the right to test horizontally
					RaycastHit[] hitsHorz = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.right, 
						numPiecesToWin - 1, 
						layermask);

					// return true (won) if enough hits
					if(hitsHorz.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// shoot a ray up to test vertically
					RaycastHit[] hitsVert = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.up, 
						numPiecesToWin - 1, 
						layermask);
					
					if(hitsVert.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// test diagonally
					if(allowDiagonally)
					{
						// calculate the length of the ray to shoot diagonally
						float length = Vector2.Distance(new Vector2(0, 0), new Vector2(numPiecesToWin - 1, numPiecesToWin - 1));

						RaycastHit[] hitsDiaLeft = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(-1 , 1), 
							length, 
							layermask);
						
						if(hitsDiaLeft.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}

						RaycastHit[] hitsDiaRight = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(1 , 1), 
							length, 
							layermask);
						
						if(hitsDiaRight.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}
					}

					yield return null;
				}

				yield return null;
			}

			// if Game Over update the winning text to show who has won
			if(gameOver == true)
			{
				winningText.GetComponent<TextMesh>().text = isPlayersTurn ? playerWonText : playerLoseText;
			}
			else 
			{
				// check if there are any empty cells left, if not set game over and update text to show a draw
				if(!FieldContainsEmptyCell())
				{
					gameOver = true;
					winningText.GetComponent<TextMesh>().text = drawText;
				}
			}

			isCheckingForWinner = false;

			yield return 0;
		}

		/// <summary>
		/// check if the field contains an empty cell
		/// </summary>
		/// <returns><c>true</c>, if it contains empty cell, <c>false</c> otherwise.</returns>
		bool FieldContainsEmptyCell()
		{
			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					if(field[x, y] == (int)Piece.Empty)
						return true;
				}
			}
			return false;
		}
	}
}
