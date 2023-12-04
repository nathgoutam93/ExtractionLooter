using System.Collections;
using UnityEngine;

public class ExtractionZone : MonoBehaviour
{
  // The time it takes to extract a player
  [SerializeField] private float extractionTime = 10f;

  // The dissolve shader to use for extraction
  [SerializeField] private Shader dissolveShader;

  // The list of players currently being extracted
  private ArrayList extractingPlayers = new ArrayList();

  private void OnTriggerEnter(Collider other)
  {
    // Check if the collider is attached to a player controller
    PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
    if (playerController != null)
    {
      StartExtraction(playerController);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    // Check if the collider is attached to a player controller and is being extracted
    PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
    if (playerController != null && extractingPlayers.Contains(playerController.gameObject))
    {
      // Cancel extraction for this player
      CancelExtraction(playerController);
    }
  }

  // Starts the extraction process for a player
  private void StartExtraction(PlayerController player)
  {
    Debug.Log("Extraction started");

    // Add the player to the list of extracting players
    extractingPlayers.Add(player.gameObject);

    // Start the dissolve effect for the player mesh
    StartCoroutine(DissolvePlayer(player.gameObject, extractionTime));
  }

  // Cancels the extraction process for a player
  private void CancelExtraction(PlayerController player)
  {
    Debug.Log("Extraction cancelled");

    // Remove the player from the list of extracting players
    extractingPlayers.Remove(player.gameObject);

    // Cancel the dissolve effect for the player mesh
    SkinnedMeshRenderer playerMesh = player.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
    StopCoroutine(DissolvePlayer(player.gameObject, extractionTime));
    StartCoroutine(RevertDissolve(playerMesh, 1f));
  }

  // Dissolves the player mesh over time
  private IEnumerator DissolvePlayer(GameObject player, float duration)
  {
    SkinnedMeshRenderer playerMesh = player.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
    Material[] originalMaterials = playerMesh.materials;

    foreach (Material material in originalMaterials)
    {
      // Switch to the dissolve shader and material
      material.shader = dissolveShader;
      material.SetVector("_DissolveOffest", new Vector3(0f, -1f, 0f));
    }

    float elapsedTime = 0f;
    while (elapsedTime < duration)
    {
      // If the player has left the extraction zone, cancel the extraction
      if (!extractingPlayers.Contains(player))
      {
        // RevertDissolve(playerMesh);
        yield break;
      }

      // Update the dissolve amount
      float dissolveAmount = elapsedTime / duration;
      Vector3 offset = Vector3.Lerp(new Vector3(0f, -1f, 0f), new Vector3(0f, 1f, 0f), dissolveAmount);
      foreach (Material material in originalMaterials)
      {
        material.SetVector("_DissolveOffest", offset);
      }
      elapsedTime += Time.deltaTime;
      yield return null;
    }

    // Finish the extraction process
    FinishExtraction(playerMesh.gameObject);
  }

  // Reverts the dissolve effect on the player mesh smoothly over a specified duration
  private IEnumerator RevertDissolve(SkinnedMeshRenderer playerMesh, float duration)
  {
    Material[] playerMaterials = playerMesh.materials;
    Vector3 originaloffset = playerMesh.materials[0].GetVector("_DissolveOffest");

    // Gradually reduce the dissolve amount back to 0 over the specified duration
    float elapsedTime = 0f;
    while (elapsedTime < duration)
    {
      float dissolveAmount = elapsedTime / duration;
      Vector3 offset = Vector3.Lerp(originaloffset, new Vector3(0f, -1f, 0f), dissolveAmount);
      foreach (Material material in playerMaterials)
      {
        material.SetVector("_DissolveOffest", offset);
      }
      Debug.Log("revert: " + offset);
      elapsedTime += Time.deltaTime;
      yield return null;
    }
  }

  // Finishes the extraction process for a player
  private void FinishExtraction(GameObject player)
  {
    Debug.Log("Extraction Finished");
    // Remove the player from the list of extracting players
    extractingPlayers.Remove(player);
  }
}
