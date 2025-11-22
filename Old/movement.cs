using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class movement : MonoBehaviour
{

    public GameObject Gun;
    private Rigidbody2D body;
    private Animator animator;
    private Animator GunAnim;
    // private Animator GunAnim;
    public Transform firePoint;
    public Transform Muzzle;
    public GameObject BulletPrefab;
    public GameObject MuzzleFlashPrefab;
    public LayerMask ClimblayerMask;
    private SpriteRenderer Srenderer;
    private float angle;
    private float horizontal;
    public float runSpeed = 20.0f;
    public float bulletSpeed = 10f;
    public float ClimbCheckDistance = 2f;
    private bool doRotate = true;
    private bool isClimbing = false;
    private bool isClimbingLeft = false;
    private bool isClimbingRight = false;
    private bool gunRight;


    
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        GunAnim = Gun.GetComponent<Animator>();
        Srenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        animator.SetBool("moving", Math.Abs(horizontal) > 0);

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = Gun.transform.position.z;  // Ensure the z-axis is the same as the turret's 
        Vector3 directionToMouse = (mouseWorldPos - Gun.transform.position).normalized; // Calculate the direction from the turret to the mouse
        angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg; // Calculate the angle to rotate the gun in the

        float mouseToPlayerX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x;
        float mouseToPlayerY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y;

        if(!isClimbing){
            if(mouseToPlayerX > 0){
                gunRight = true;
            }else{
                gunRight = false;
            }
        }else if(isClimbingRight){
            if(mouseToPlayerY > 0){
                gunRight = true;
            }else{
                gunRight = false;
            }
        }else if(isClimbingLeft){
            if(mouseToPlayerY < 0){
                gunRight = true;
            }else{
                gunRight = false;
            }
        }
        
        
        if(doRotate){
            Gun.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); // Set the gun's rotation to face the mouse, considering the turret's orientation
        }else{
            Vector3 direction = gunRight ? transform.right : -transform.right;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Gun.transform.rotation = Quaternion.Euler(0, 0, angle);
            // Gun.transform.rotation = gunRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180);
        }


        if(!isClimbing){
            if(angle > 0){
                doRotate = true;
            }else{
                doRotate = false;
            }
        }else{
            if(isClimbingRight){
                if(Mathf.Abs(angle) < 90){
                    doRotate = false;
                }else{
                    doRotate = true;
                }
            }else if(isClimbingLeft){
                if(Mathf.Abs(angle) > 90){
                    doRotate = false;
                }else{
                    doRotate = true;        
                }
            }
            
        }
        
        if (Input.GetMouseButtonDown(0)){
            Shoot(directionToMouse);
        }

        if(horizontal < 0){
            Flip(true);
        }else if(horizontal > 0){
            Flip(false);
        }

        RaycastHit2D RightClimbRay = Physics2D.Raycast(transform.position, transform.right, ClimbCheckDistance, ClimblayerMask);
        RaycastHit2D LeftClimbRay = Physics2D.Raycast(transform.position, -transform.right, ClimbCheckDistance, ClimblayerMask);
        RaycastHit2D GroundCheck = Physics2D.Raycast(transform.position, -transform.up, ClimbCheckDistance, ClimblayerMask);
        Debug.DrawRay(transform.position, transform.right * ClimbCheckDistance, Color.red);
        Debug.DrawRay(transform.position, -transform.right * ClimbCheckDistance, Color.red);
        Debug.DrawRay(transform.position, -transform.up * ClimbCheckDistance, Color.red);
        
        if(RightClimbRay.collider != null && !isClimbing && horizontal != 0){
            Collider2D hitCol = RightClimbRay.collider;

            if(hitCol.CompareTag("ClimbWall") || hitCol.CompareTag("Ground")){
                isClimbing = true;
                isClimbingRight = true;

                Vector2 wallNormal = RightClimbRay.normal;
                
                transform.up = wallNormal;
            }   
        }
        else if(LeftClimbRay.collider != null && !isClimbing && horizontal != 0){
            Collider2D hitCol = LeftClimbRay.collider;

            if(hitCol.CompareTag("ClimbWall") || hitCol.CompareTag("Ground")){
                isClimbing = true;
                isClimbingLeft = true;

                Vector2 wallNormal = LeftClimbRay.normal;
                
                transform.up = wallNormal;
            }   
        }
        else if(horizontal != 0 && isClimbing){
            if(RightClimbRay.collider != null || LeftClimbRay.collider != null){
                Collider2D hitCol = RightClimbRay.collider != null ? RightClimbRay.collider : LeftClimbRay.collider;
                if(hitCol.CompareTag("ClimbWall") || hitCol.CompareTag("Ground")){
                    isClimbing = false;
                    isClimbingRight = false;
                    isClimbingLeft = false;

                    Vector2 groundNormal = Vector2.up;
                    transform.up = groundNormal;
                }
            } 
        }

        if(isClimbing && horizontal > 0){
            if(RightClimbRay.collider != null){
                Collider2D hitCol = RightClimbRay.collider;
                if(hitCol.CompareTag("ClimbBarrier") && GroundCheck.collider != null){
                    if(GroundCheck.collider.CompareTag("ClimbWall")){
                        if(GroundCheck.collider != null){
                            Transform ground = GroundCheck.collider.transform;
                            Vector2 snapPos = ground.Find("ClimbSnap").transform.position;
                            isClimbing = false;

                            transform.position = snapPos;
                            Vector2 groundNormal = Vector2.up;
                            transform.up = groundNormal;
                            body.linearVelocity = new Vector2(horizontal * runSpeed, 0);
                        }
                    }
                }
            }
        }
        if(isClimbing && horizontal < 0){
            if(LeftClimbRay.collider != null){
                Collider2D hitCol = RightClimbRay.collider != null ? RightClimbRay.collider : LeftClimbRay.collider;
                if(hitCol.CompareTag("ClimbBarrier") && GroundCheck.collider != null){
                    if(GroundCheck.collider.CompareTag("ClimbWall")){
                        if(GroundCheck.collider != null){
                            Transform ground = GroundCheck.collider.transform;
                            Vector2 snapPos = ground.Find("ClimbSnap").transform.position;


                            isClimbing = false;

                            transform.position = snapPos;
                            Vector2 groundNormal = Vector2.up;
                            transform.up = groundNormal;
                            body.linearVelocity = new Vector2(horizontal * runSpeed, 0);
                        }
                    }
                }
            }
        }
        else if(!isClimbing && horizontal != 0){
            if(LeftClimbRay.collider != null){
                if(LeftClimbRay.collider.CompareTag("LeftClimbBarrier") && GroundCheck.collider != null){
                    if(GroundCheck.collider.CompareTag("ClimbWall") || GroundCheck.collider.CompareTag("Ground")){
                        if(GroundCheck.collider != null){
                            Transform ground = GroundCheck.collider.transform;
                            Vector2 snapPos = ground.Find("ReturnClimbSnap").transform.position;

                            isClimbing = true;
                            isClimbingRight = false;
                            isClimbingLeft = true;

                            transform.position = snapPos;
                            Vector2 groundNormal = Vector2.up;
                            transform.up = groundNormal;
                        }
                    }
                }
            }
            else if(RightClimbRay.collider != null){
                if(RightClimbRay.collider.CompareTag("LeftClimbBarrier") && GroundCheck.collider != null){
                    if(GroundCheck.collider.CompareTag("ClimbWall") || GroundCheck.collider.CompareTag("Ground")){
                        if(GroundCheck.collider != null){
                            Transform ground = GroundCheck.collider.transform;
                            Vector2 snapPos = ground.Find("ReturnClimbSnap").transform.position;

                            isClimbing = true;
                            isClimbingRight = true;
                            isClimbingLeft = false;

                            transform.position = snapPos;
                            Vector2 groundNormal = Vector2.up;
                            transform.up = groundNormal;
                        }
                    }
                }
            }
        }
    }

    private void Shoot(Vector3 direction){
        GameObject MuzzleFlash = Instantiate(MuzzleFlashPrefab, Muzzle.position, Gun.transform.rotation);
        MuzzleFlash.transform.SetParent(Muzzle, true);
        Destroy(MuzzleFlash, 0.1f);

        GameObject projectile = Instantiate(BulletPrefab, firePoint.position, Gun.transform.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if(doRotate){
            if (rb != null){
                rb.linearVelocity = direction * bulletSpeed;
            }
        }else{
            if (rb != null){
                rb.linearVelocity = (gunRight ? transform.right : -transform.right) * bulletSpeed;
            }
        }
        Destroy(projectile, 2f);
    }

    private void FixedUpdate()
    {
        if(isClimbing){
            if(isClimbingRight){
                body.linearVelocity = new Vector2(0, horizontal * runSpeed);
            }
            else if(isClimbingLeft){
                body.linearVelocity = new Vector2(0, -horizontal * runSpeed);
            }
        }else{
            body.linearVelocity = new Vector2(horizontal * runSpeed, 0);
        }
    }

    void Flip(bool flipx){
        Srenderer.flipX = flipx;
    }
}