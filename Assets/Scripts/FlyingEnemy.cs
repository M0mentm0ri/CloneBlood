using System;
using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [Header("��s�p�ݒ�")]
    public float hoverHeight = 5f;
    public bool keepFixedHeight = true;

    [Header("�U���p�ݒ�")]
    public float attackRange = 10f; // �^�[�Q�b�g�ɋ߂Â����Ƃ��U�����鋗��
    public float offsetAngle = 10f;  // �C���X�y�N�^�Őݒ�\�ȃI�t�Z�b�g�p�x�iY�������j
    public ParticleSystem currentParticle;
    public Vector3 attackDirection; // �U������


    private bool isAttacking = false; // �U�������ǂ����𔻒肷��t���O

    protected override void Update()
    {
        // ���t���[���A�U��������ɍs��
        CheckAttack();

        // �U�����Ă��Ȃ��Ƃ��݈̂ړ�
        if (!isAttacking)
        {
            Move();
        }
    }

    protected override void Move()
    {
        if (targetPosition == null) return;

        // ���ړ��������l���iY�����͌Œ�j
        Vector3 direction = targetPosition.position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);

        if (distanceToTarget > stoppingDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;

            // �i�s�����������iX���������]������j
            if (direction != Vector3.zero)
            {
                Vector3 currentScale = transform.localScale;
                if (direction.x > 0)
                    transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // ������
                else if (direction.x < 0)
                    transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // �E����
            }

            // �������Œ�
            if (keepFixedHeight)
            {
                Vector3 pos = transform.position;
                pos.y = hoverHeight;
                transform.position = pos;
            }
        }
    }

    void CheckAttack()
    {
        isAttacking = false; // ���t���[��������

        // �U�������͂Ƃ肠�����^�[�Q�b�g�����ɂ��Ă����i�������Ă��U���ł���悤�Ɂj
        if (targetPosition != null)
        {
            attackDirection = (targetPosition.position - transform.position).normalized;
        }

        // ������ �^���ɃI�t�Z�b�g�����ʒu�𒆐S�ɔ͈͌��m
        Vector3 detectCenter = transform.position + Vector3.down * 1f; // ��������1m��
        LayerMask playerMask = LayerMask.GetMask("Player");

        Collider[] hits = Physics.OverlapSphere(detectCenter, sphereRadius, playerMask);
        if (hits.Length > 0)
        {
            isAttacking = true;
            animator.SetBool("IsAttack", true);

            // �v���C���[�������U�������ɐݒ�
            attackDirection = (hits[0].transform.position - transform.position).normalized;

            // �p�[�e�B�N������]������
            if (currentParticle != null)
            {
                // X�������ɃI�t�Z�b�g��������
                Vector3 offsetDirection = new Vector3(offsetAngle, 0, 0); // X���ɑ΂���I�t�Z�b�g���쐬

                // attackDirection �ɃI�t�Z�b�g�𑫂�
                attackDirection += offsetDirection;  // X�������ɃI�t�Z�b�g�𑫂�


                currentParticle.transform.rotation = Quaternion.LookRotation(attackDirection);
            }

            return; // �U������
        }

        // �^�[�Q�b�g�n�_���߂���΂�����ɍU��
        if (targetPosition != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);
            if (distanceToTarget <= attackRange)
            {
                isAttacking = true;
                animator.SetBool("IsAttack", true);

                attackDirection = (targetPosition.position - transform.position).normalized;

                // X�������ɃI�t�Z�b�g��������
                Vector3 offsetDirection = new Vector3(offsetAngle, 0, 0); // X���ɑ΂���I�t�Z�b�g���쐬

                // attackDirection �ɃI�t�Z�b�g�𑫂�
                attackDirection += offsetDirection;  // X�������ɃI�t�Z�b�g�𑫂�

                if (currentParticle != null)
                {
                    currentParticle.transform.rotation = Quaternion.LookRotation(attackDirection);
                }

                return;
            }
        }

        animator.SetBool("IsAttack", false);  // �U������
    }

    public void Fire()
    {
        if (currentParticle != null)
        {
            // ��~�ƍĐ��i�����ŏ��߂ă��Z�b�g�����j
            currentParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentParticle.Play();
        }
    }

    protected override bool DetectTarget()
    {
        if (targetPosition == null) return false;

        // �v���C���[�Ƃ̋������v�Z
        float distance = Vector3.Distance(transform.position, targetPosition.position);

        // �w��͈͓����uPlayer�v���C���[�̂݌��m
        if (distance < detectionRange && targetPosition.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            return true;
        }

        return false;
    }

    // Editor���SphereCast�̌��m�͈͂�����
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 detectCenter = transform.position + Vector3.down * 1f;
        Gizmos.DrawWireSphere(detectCenter, sphereRadius);
    }
}
