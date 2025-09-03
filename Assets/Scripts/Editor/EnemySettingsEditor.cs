using Utilities.Enums;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySettings))]
public class EnemySettingsEditor : Editor
{
    SerializedProperty enemyName;
    SerializedProperty enemyType;
    SerializedProperty moveSpeed;
    SerializedProperty stoppingDistance;
    SerializedProperty acceleration;
    SerializedProperty angularSpeed;
    SerializedProperty attackDamage;
    SerializedProperty startHealth;
    SerializedProperty headshotMultiplier;
    SerializedProperty bodyshotMultiplier;
    SerializedProperty limbMultiplier;
    SerializedProperty attackRange;
    SerializedProperty attackCooldown;
    SerializedProperty attackWindupTime;
    SerializedProperty safeDistance;         
    SerializedProperty minAttackDistance;     
    SerializedProperty panicDistance;         
    SerializedProperty fireRate;              
    SerializedProperty initialPoolSize;          
    SerializedProperty projectileSpeed;
    SerializedProperty maxProjectileDistance;
    SerializedProperty panicMoveRadius;   
    SerializedProperty panicRecalcTime;   
    SerializedProperty panicPointSearchAttempts; 
    SerializedProperty panicSpeed;             
    SerializedProperty retreatDuration;    
    SerializedProperty retreatStep;            
    SerializedProperty stateChangeCooldown;
    SerializedProperty dieDisabledtime;

    private void OnEnable()
    {
        enemyName = serializedObject.FindProperty("enemyName");
        enemyType = serializedObject.FindProperty("enemyType");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        stoppingDistance = serializedObject.FindProperty("stoppingDistance");
        acceleration = serializedObject.FindProperty("acceleration");
        angularSpeed = serializedObject.FindProperty("angularSpeed");
        attackDamage = serializedObject.FindProperty("attackDamage");
        startHealth = serializedObject.FindProperty("startHealth");
        headshotMultiplier = serializedObject.FindProperty("headshotMultiplier");
        bodyshotMultiplier = serializedObject.FindProperty("bodyshotMultiplier");
        limbMultiplier = serializedObject.FindProperty("limbMultiplier");
        attackRange = serializedObject.FindProperty("attackRange");
        attackCooldown = serializedObject.FindProperty("attackCooldown");
        attackWindupTime = serializedObject.FindProperty("attackWindupTime");
        safeDistance = serializedObject.FindProperty("safeDistance");
        minAttackDistance = serializedObject.FindProperty("minAttackDistance");
        panicDistance = serializedObject.FindProperty("panicDistance");
        fireRate = serializedObject.FindProperty("fireRate");
        initialPoolSize = serializedObject.FindProperty("initialPoolSize");
        projectileSpeed = serializedObject.FindProperty("projectileSpeed");
        maxProjectileDistance = serializedObject.FindProperty("maxProjectileDistance");
        panicMoveRadius = serializedObject.FindProperty("panicMoveRadius");
        panicRecalcTime = serializedObject.FindProperty("panicRecalcTime");
        panicPointSearchAttempts = serializedObject.FindProperty("panicPointSearchAttempts");
        panicSpeed = serializedObject.FindProperty("panicSpeed");
        retreatDuration = serializedObject.FindProperty("retreatDuration");
        retreatStep = serializedObject.FindProperty("retreatStep");
        stateChangeCooldown = serializedObject.FindProperty("stateChangeCooldown");
        dieDisabledtime = serializedObject.FindProperty("dieDisabledtime");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EnemyType currentType = (EnemyType)enemyType.enumValueIndex;

        //Move
        EditorGUILayout.PropertyField(enemyName);
        EditorGUILayout.PropertyField(enemyType);
        EditorGUILayout.PropertyField(moveSpeed);
        EditorGUILayout.PropertyField(stoppingDistance);
        EditorGUILayout.PropertyField(acceleration);
        EditorGUILayout.PropertyField(angularSpeed);

        //Health
        EditorGUILayout.PropertyField(startHealth);

        //Attack
        EditorGUILayout.PropertyField(attackDamage);
        switch (currentType)
        {
            case EnemyType.Melle:
                EditorGUILayout.PropertyField(attackRange);
                EditorGUILayout.PropertyField(attackCooldown);
                EditorGUILayout.PropertyField(attackWindupTime);
                break;
            case EnemyType.Range:
                EditorGUILayout.PropertyField(safeDistance);
                EditorGUILayout.PropertyField(minAttackDistance);
                EditorGUILayout.PropertyField(fireRate);
                EditorGUILayout.PropertyField(initialPoolSize);
                EditorGUILayout.PropertyField(projectileSpeed);
                EditorGUILayout.PropertyField(maxProjectileDistance);
                EditorGUILayout.PropertyField(panicDistance);
                EditorGUILayout.PropertyField(panicMoveRadius);
                EditorGUILayout.PropertyField(panicRecalcTime);
                EditorGUILayout.PropertyField(panicPointSearchAttempts);
                EditorGUILayout.PropertyField(panicSpeed);
                EditorGUILayout.PropertyField(retreatDuration);
                EditorGUILayout.PropertyField(retreatStep);
                EditorGUILayout.PropertyField(stateChangeCooldown);
                break;
        }

        //Damage multi
        EditorGUILayout.PropertyField(headshotMultiplier);
        EditorGUILayout.PropertyField(bodyshotMultiplier);
        EditorGUILayout.PropertyField(limbMultiplier);

        //After Dead
        EditorGUILayout.PropertyField(dieDisabledtime);

        serializedObject.ApplyModifiedProperties();
    }
}
