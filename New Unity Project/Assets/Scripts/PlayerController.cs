using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f; // скорость движения персонажа
    public float gravityMultiplier = 2f; // Множитель силы тяжести
    public float jumpForce = 7.0f; // Сила прыжка
    private CharacterController controller;
    private float verticalVelocity = -30f;
    public bool isInputEnabled = true;

    // Event function
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Event function
    private void Update()
    {
        if (!isInputEnabled) return;
        // Получаем ввод от игрока
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Вычисляем направление движения по горизонтали
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        // Обработка прыжка
        if (controller.isGrounded)
        {
            verticalVelocity = -1f; // Небольшое отрицательное значение, чтобы гарантировать isGrounded
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y * gravityMultiplier);
            }
        }
        else
        {
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }

        // Применяем вертикальную скорость к направлению движения
        moveDirection.y = verticalVelocity;

        // Двигаем персонажа
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}