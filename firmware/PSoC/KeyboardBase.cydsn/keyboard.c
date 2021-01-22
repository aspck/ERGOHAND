/*********************************************
 *  File:       keyboard.c
 *  Author:     ***REMOVED***
 *  Created:    18-Jan-2021 
 *
 ********************************************/

#include "keyboard.h"

uint8_t keyMap[nMAXKEYS] = {
    0x04,   0x05,   0x06,   0x07,   0x08,
    0x09,   0x0a,   0x0b,   0x0c,   0x0d,
    0x0e,   0x0f,   0x10,   0x11,   0x12,
    0x13,   0x14,   0x15,   0x16,   0x17,
    0x18,   0x19,   0x1a,   0x1b,   0x1c,   0xff
};  

uint8_t Keyboard_ScanKeys() 
{
    static uint8_t keysDebounce[nROWS][nCOLS] = {0};
    uint8_t bKeyChanged = 0;
    uint8_t keyBuf = 0;
    
    /* iterate over rows */
    for (uint8_t row = 0; row < nROWS; row++)
    {
        /* activate current row */
        outPort_Write(1u << row);
        CyDelayUs(10);        
        keyBuf = inPort_Read();
        
        /* iterate over every key, even if no keypress detected, to update the debounce counters */
        for (uint8_t col = 0; col < nCOLS; col++)
        {            
            if (keyBuf & (1u << col)) /* key down */
            {   
                if (keysDebounce[row][col] < DEBOUNCE_COUNT)
                {
                    keysDebounce[row][col]++;
                }
                else if (keysDebounce[row][col] == DEBOUNCE_COUNT)
                {
                    /* debounce threshold met, add key to report and set flag */
                    bKeyChanged = 1;            
                    uint8_t keycode = getKeycodeFromIndex(row, col);
                    HID_Report_AddKey(keycode);
                    DBG_PRINTF("key down: %d \r\n", (row * nCOLS + col));
                    keysDebounce[row][col]++;
                }

                /* if key is already down and debounced, do nothing */
                    
            }
            else if (keysDebounce[row][col] > DEBOUNCE_COUNT) /* key up */
            {
                bKeyChanged = 1;    
                keysDebounce[row][col] = 0;
                uint8_t keycode = getKeycodeFromIndex(row, col);
                HID_Report_RemoveKey(keycode);
                DBG_PRINTF("key up: %d \r\n", (row * nCOLS + col));
            }
        }
        
    }
    
    return bKeyChanged;
}

uint8_t Keyboard_UpdateConfigAtt()
{
    /* create struct to send */
    CYBLE_GATT_HANDLE_VALUE_PAIR_T locHandleValuePair;
    locHandleValuePair.attrHandle = KEYMAP_ATTR1;
    locHandleValuePair.value.len = nMAXKEYS/2;
    locHandleValuePair.value.val = keyMap;
    
    volatile CYBLE_GATT_ERR_CODE_T result;
    /* since this characteristic is "long" we need to do 2 writes */
    result = CyBle_GattsWriteAttributeValue(&locHandleValuePair, 0u, NULL, CYBLE_GATT_DB_LOCALLY_INITIATED);
    {
        DBG_PRINTF("wrote attribute: %x \r\n",*locHandleValuePair.value.val);
        locHandleValuePair.attrHandle = KEYMAP_ATTR2;
        locHandleValuePair.value.val = keyMap + nMAXKEYS/2;
        if( CyBle_GattsWriteAttributeValue(&locHandleValuePair, 0u, NULL, CYBLE_GATT_DB_LOCALLY_INITIATED) == CYBLE_GATT_ERR_NONE )
        {
            DBG_PRINTF("wrote attribute: %x \r\n ",*(locHandleValuePair.value.val));
            return 0;
        }
    }

    return 1;
}        
        
uint8_t Keyboard_WriteConfig()
{
    cy_en_em_eeprom_status_t result = EEPROM_Write(0u, keyMap, nMAXKEYS);
    if (result == CY_EM_EEPROM_SUCCESS)
    {
        return 0;
    } else {
        return 1;
    }    
}

uint8_t Keyboard_ReadConfig()
{
    uint8_t keyMapBuf[nMAXKEYS];
    cy_en_em_eeprom_status_t result = EEPROM_Read(0u, keyMapBuf, nMAXKEYS);
    if (result == CY_EM_EEPROM_SUCCESS)
    {
        memcpy(keyMap, keyMapBuf, nMAXKEYS);
        return 0;
    } else {
        return 1;
    }   

}

CYBLE_API_RESULT_T HID_Report_Send()
{
    CYBLE_API_RESULT_T apiResult = CYBLE_ERROR_STACK_BUSY;
    
    if(CyBle_GattGetBusyStatus() == CYBLE_STACK_STATE_FREE)
    {
        /* determine if we're in Boot Device mode */
        apiResult = CyBle_HidssGetCharacteristicValue(CYBLE_HUMAN_INTERFACE_DEVICE_SERVICE_INDEX, 
            CYBLE_HIDS_PROTOCOL_MODE, sizeof(protocol), &protocol);
        
        if(apiResult == CYBLE_ERROR_OK)
        {
            DBG_PRINTF("HID notification: ");
            for(uint i = 0; i < KEYBOARD_DATA_SIZE; i++)
            {
                DBG_PRINTF("%2.2x,", key_data[i]);
            }
            DBG_PRINTF("\r\n");
            
            /* send HID service notification to HID host */
            if(protocol == CYBLE_HIDS_PROTOCOL_MODE_BOOT)
            {
                apiResult = CyBle_HidssSendNotification(cyBle_connHandle, CYBLE_HUMAN_INTERFACE_DEVICE_SERVICE_INDEX,
                    CYBLE_HIDS_BOOT_KYBRD_IN_REP, KEYBOARD_DATA_SIZE, key_data);
            }
            else
            {
                apiResult = CyBle_HidssSendNotification(cyBle_connHandle, CYBLE_HUMAN_INTERFACE_DEVICE_SERVICE_INDEX, 
                    CYBLE_HUMAN_INTERFACE_DEVICE_REPORT_IN, KEYBOARD_DATA_SIZE, key_data);
            }
            
            if(apiResult != CYBLE_ERROR_OK)
            {
                DBG_PRINTF("HID notification API Error: %x \r\n", apiResult);
                //keyboardSimulation = DISABLED;
            }
        }
    }
    return apiResult;

}

void HID_Report_RemoveKey(uint8_t keycode)
{
    for (uint i = 2; i < KEYBOARD_DATA_SIZE; i++)
    {
        if (key_data[i] == keycode)
        {
            key_data[i] = 0u;   
            break;
        }
    }
}

void HID_Report_AddKey(uint8_t keycode)
{    
    for (uint i = 2; i < KEYBOARD_DATA_SIZE; i++)
    {
        if (key_data[i] == 0u)
        {
            key_data[i] = keycode;
            break;
        }
    }
}
/* [] END OF FILE */
