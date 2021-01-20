/*********************************************
 *  File:       keyboard.c
 *  Author:     ***REMOVED***
 *  Created:    18-Jan-2021 
 *
 ********************************************/

#include "keyboard.h"

uint8_t keyMap[nMAXKEYS] = {
    0x04,   0x05,   0x06,   0x07,
    0x1E, 0x1F, 0x20, 0x21,
    0,  0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0  
};  

/*** Polls all keys, keeps track of key states, returns change flag */
uint8_t ScanKeys()
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

/* Notifies the HID host of a keyboard report change
*/
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

/*** Searches for the first instance of a keycode in the HID report and clears the status
*   TODO: move active keys to front of queue
*/
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

/*** Adds a keycode to the first available empty slot in the HID report*/
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
