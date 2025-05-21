using UnityEngine;

public class EInteraction : MonoBehaviour
{
    public Animator switchAnimator;
    public string switchAnimationName = "SwitchAnimation";

    public Animator doorAnimator;
    public string doorOpenAnimationName = "DoorOpen";

    public float interactionDistance = 2f;
    private Transform playerTransform;
    private bool switchActivated = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Не найден игровой объект с тегом 'Player'.");
            enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distanceToSwitch = Vector3.Distance(playerTransform.position, switchAnimator.transform.position);

            if (Input.GetKeyDown(KeyCode.E) && distanceToSwitch <= interactionDistance)
            {
                if (!switchActivated && switchAnimator != null)
                {
                    switchAnimator.Play(switchAnimationName);
                    switchActivated = true;
                    Invoke("PlayDoorAnimation", 1f);
                }
                // Больше нет логики для закрытия двери
            }

            if (distanceToSwitch <= interactionDistance)
            {
                // Здесь можно добавить код для отображения подсказки "Нажмите 'E'"
            }
            else
            {
                // Здесь можно добавить код для скрытия подсказки
            }
        }
    }

    void PlayDoorAnimation()
    {
        if (doorAnimator != null && !string.IsNullOrEmpty(doorOpenAnimationName))
        {
            doorAnimator.SetTrigger("OpenDoor");
        }
        else
        {
            Debug.LogWarning("Аниматор двери не назначен или не указано название анимации открытия.");
        }
    }
}