using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Vector2 _face;
    private Vector2 _aim;
    private Animator _animator;
    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";
    private const string _facehorizontal = "FaceHorizontal";
    private const string _facevertical = "FaceVertical";

    //Mouse Input Section
    [Header("Mouse")]
    [SerializeField] private bool CheckMouse;
    [SerializeField] private bool MouseInput;
    public float inactivityThreshold = 5f; // Time in seconds before the cursor is hidden
    [SerializeField] private Vector3 lastMousePosition;
    [SerializeField] private float inactivityTimer;

    [Header("UI")]
    public float radius = 1f;
    [SerializeField] private GameObject AimArrowGO;
    [SerializeField] private Transform AimArrow;
    [SerializeField] private Transform player;
    protected float desieredAngle;

    [Header("Attack")]
    // Cooldown management
    [SerializeField] private float shootCooldown = 0.5f;
    private float nextShootTime = 0f;

    // Bullet properties
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletDamage = 10f;

    // Firing mechanics
    [SerializeField] private Transform firePoint;

    //effects
    [SerializeField] private AudioClip shootingSound;
    private AudioSource audioSource;

    // Start is called before the first frame update
    private void Awake()
    {

    }
    void Start()
    {
        _animator = GetComponent<Animator>();
        if (PlatformManager.Instance.CheckPlatform() == 2 || PlatformManager.Instance.CheckPlatform() == 3)
        {
            CheckMouse = false;
            MouseInput = false;
        }
        else
        {
            CheckMouse = true;
            lastMousePosition = Input.mousePosition;
            inactivityTimer = 0f;
            Cursor.visible = true;
            MouseInput = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (CheckMouse)
        {
            CheckMouseActive();
        }
        if (MouseInput)
        {
            _aim.Set(InputManager.Aim.x, InputManager.Aim.y);
        }
        _face.Set(InputManager.Face.x, InputManager.Face.y);

        //? so basically this one is saying if there is no joystick input,
        if (_face == Vector2.zero)
        {
            if (!MouseInput)
            {
                AimArrowGO.SetActive(false);
            }
            else
            {
                AimArrowGO.SetActive(true);
            }
            UpdateMouseAimArrowPos(_aim);
        }
        else
        {
            AimArrowGO.SetActive(true);
            UpdateStickAimArrowPos(_face);
        }

        if (InputManager.shooting > 0)
        {
            Vector2 bulletDir = firePoint.position - player.position;
            Shoot(bulletDir);
            if (InputManager.Movement == Vector2.zero)
            {

                if (_face == Vector2.zero)
                {
                    Vector2 NormalizedAim = NormalizeScreenXY(_aim);
                    _animator.SetFloat(_facehorizontal, NormalizedAim.x);
                    _animator.SetFloat(_facevertical, NormalizedAim.y);

                }
                else
                {

                    _animator.SetFloat(_facehorizontal, _face.x);
                    _animator.SetFloat(_facevertical, _face.y);
                }
            }
        }

    }

    private void UpdateStickAimArrowPos(Vector2 pos)
    {
        //Desable Cursor
        Cursor.visible = false;
        desieredAngle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        AimArrow.rotation = Quaternion.AngleAxis(desieredAngle, Vector3.forward);
    }

    private void UpdateMouseAimArrowPos(Vector2 Pos)
    {
        Cursor.visible = true;
        Vector2 NormalizedPos = NormalizeScreenXY(Pos);
        desieredAngle = Mathf.Atan2(NormalizedPos.y, NormalizedPos.x) * Mathf.Rad2Deg;
        AimArrow.rotation = Quaternion.AngleAxis(desieredAngle, Vector3.forward);
    }

    private Vector2 NormalizeScreenXY(Vector2 Pos)
    {
        float normalizedX = (Pos.x / Screen.width) * 2 - 1;
        float normalizedY = (Pos.y / Screen.height) * 2 - 1;

        return new Vector2(normalizedX, normalizedY);
    }


    private void CheckMouseActive()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        // Check if the mouse has moved
        if (currentMousePosition != lastMousePosition)
        {
            // Mouse moved: reset timer and show cursor
            MouseInput = true;
            inactivityTimer = 0f;
            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }
        else
        {
            // Mouse not moved: increment timer
            Debug.Log("Mouse Not Moving");
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold && Cursor.visible)
            {
                Cursor.visible = false; // Hide cursor after inactivity
                MouseInput = false;
            }
        }

        // Update the last mouse position
        lastMousePosition = currentMousePosition;
    }

    private void Shoot(Vector2 dir)
    {
        // Check if the shooting cooldown is up
        if (!CanShoot())
            return;

        // Reset the cooldown
        ResetShootCooldown();

        // Spawn bullet at the fire point
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        // Check if the bullet has a Bullet component to configure its properties
        PlayerBullet bullet = bulletGO.GetComponent<PlayerBullet>();

        if (bullet != null)
        {
            // Set the direction and damage for the bullet
            bullet.SetDirection(dir);
            bullet.SetDamage(bulletDamage);
        }
        else
        {
            Debug.LogWarning("Spawned bullet does not have a Bullet component.");
        }

        //PlayShootingEffect();
    }

    private bool CanShoot()
    {
        return Time.time >= nextShootTime;
    }

    private void ResetShootCooldown()
    {
        nextShootTime = Time.time + shootCooldown;
    }

}
