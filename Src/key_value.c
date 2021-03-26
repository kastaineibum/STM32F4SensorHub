#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdarg.h>
#include <stdbool.h>
#include "key_value.h"
#include <stdlib.h>

#define KEY_DEBUG 1

#if KEY_DEBUG

#define KEY_VALUE_INFO( fmt, args... ) 	printf( fmt, ##args )     //rtt print info  SEGGER_RTT_printf( 0, fmt, ##args )
#else
#define KEY_VALUE_INFO( fmt, args... )
#endif

/************************Simple transplantable flash operation**********************/

#if SYS
#include "cmsis_os.h"
SemaphoreHandle_t key_value_SemaphoreHandle;
#endif

#define KEY_VALUE_MAX_SIZE   ( KEY_VALUE_SIZE * SECTOR_NUM )

enum BACKUP_FLAG{
    BACKUP_FLAG_CLEAR   = BACKUP_FLAG_CLEAR_FLAG,
    UINT32_FLAG         = 0x00009600,
    STRINGS_FLAG        = 0x00006900,
};

//Only two kinds of key-value are supported;    namely int or string type
uint32_t KEY_VALUE_INT32 = 0;
uint32_t KEY_VALUE_BACKUP = 0;
uint32_t KEY_VALUE_STRINGS = 0;
uint8_t rewrite_times = 0;

bool backup_flag( enum TYPE type, bool stat );
uint8_t get_backup_flag( enum TYPE type );
bool move_key_value_back( enum TYPE type );

void BubbleSort( uint32_t array[], int n ){
    int i, j , flag =1;
    uint32_t temp;
    for( i = 1; i < n && flag == 1; i ++ ){
        flag = 0;
        for( j = 0; j < n - i ; j ++ ){
            if( array[j] > array[j+1] ){
                flag = 1;
                temp = array[j];
                array[j] = array[j+1];
                array[j+1] = temp;
            }
        }
    }
}

//3*128*1024 + FLASH_BASE + 64*1024
void init_key_value2(void){

    KEY_VALUE_INT32 = 3*128*1024 + FLASH_BASE + 64*1024;
    KEY_VALUE_STRINGS = 3*128*1024 + FLASH_BASE + 80*1024;
		KEY_VALUE_BACKUP = 3*128*1024 + FLASH_BASE + 96*1024;
    
    uint32_t array[3] = { KEY_VALUE_INT32, KEY_VALUE_STRINGS, KEY_VALUE_BACKUP };
    
    BubbleSort( array, 3 );
    
    if( ( array[0] != 0 && ( array[0] + SECTOR_NUM * KEY_VALUE_SIZE) > array[1] ) || ( array[1] + SECTOR_NUM * KEY_VALUE_SIZE) > array[2] ){
        KEY_VALUE_INFO(" System allocation error exists cross flash memory\r\n");
        while( true );
    }

    #if SYS
        key_value_SemaphoreHandle = xSemaphoreCreateBinary();//signal
        xSemaphoreGive( key_value_SemaphoreHandle );
        if( key_value_SemaphoreHandle == NULL ){
            KEY_VALUE_INFO(" init_key_value key_value_SemaphoreHandle failed\r\n");
            while( true );
        }
    #endif
    
    #if 0
        if( KEY_VALUE_INT32 )
            flash_erase( key_value_int32, SECTOR_NUM );
        if( KEY_VALUE_STRINGS )
            flash_erase( key_value_string, SECTOR_NUM );
        if( KEY_VALUE_BACKUP )
            flash_erase( key_value_backup, SECTOR_NUM );
    #endif
    
    uint32_t uint32_flag = 0;
    uint32_t strings_flag = 0;
    
    if( KEY_VALUE_INT32 ){
        get_key_value( "UINT32_INIT_FLAG", UINT32, ( uint8_t *)&uint32_flag );
        if( uint32_flag != UINT32_INIT_FLAG ){
            flash_erase( KEY_VALUE_INT32, SECTOR_NUM );
            uint32_flag = UINT32_INIT_FLAG;
            set_key_value( "UINT32_INIT_FLAG", UINT32, ( uint8_t *)&uint32_flag );
        }
    }

    if( KEY_VALUE_STRINGS ){
        get_key_value( "STRINGS_INIT_FLAG", STRINGS, ( uint8_t *)&strings_flag );
        if( strings_flag == 0 || memcmp( (char *)strings_flag, STRINGS_INIT_FLAG, strlen( STRINGS_INIT_FLAG ) ) != 0 ){
            flash_erase( KEY_VALUE_STRINGS, SECTOR_NUM );
            set_key_value( "STRINGS_INIT_FLAG", STRINGS, ( uint8_t *)STRINGS_INIT_FLAG );
        }
    }

    if( KEY_VALUE_INT32 != 0 ){                         //Determine whether the address is valid or not, within the chip address range. 
        if( get_backup_flag( UINT32 ) ){
            move_key_value_back( UINT32 );
        }
    }
    
    if( KEY_VALUE_STRINGS != 0 ){
        if( get_backup_flag( STRINGS ) ){
            move_key_value_back( STRINGS );
        }
    }
    
    if( KEY_VALUE_BACKUP != 0 ){
        backup_flag( UINT32, false );
    }
}


void init_key_value( uint8_t key_value_int32_flash_index, uint8_t key_value_string_flash_index, uint8_t key_value_backup_flash_index ){

    KEY_VALUE_INT32 = flash_sector_address(key_value_int32_flash_index);
    KEY_VALUE_STRINGS = flash_sector_address(key_value_string_flash_index);
	KEY_VALUE_BACKUP = flash_sector_address(key_value_backup_flash_index);
    
    uint32_t array[3] = { KEY_VALUE_INT32, KEY_VALUE_STRINGS, KEY_VALUE_BACKUP };
    
    BubbleSort( array, 3 );
    
    if( ( array[0] != 0 && ( array[0] + SECTOR_NUM * KEY_VALUE_SIZE) > array[1] ) || ( array[1] + SECTOR_NUM * KEY_VALUE_SIZE) > array[2] ){
        KEY_VALUE_INFO(" System allocation error exists cross flash memory\r\n");
        while( true );
    }

    #if SYS
        key_value_SemaphoreHandle = xSemaphoreCreateBinary();//signal
        xSemaphoreGive( key_value_SemaphoreHandle );
        if( key_value_SemaphoreHandle == NULL ){
            KEY_VALUE_INFO(" init_key_value key_value_SemaphoreHandle failed\r\n");
            while( true );
        }
    #endif
    
    #if 0
        if( KEY_VALUE_INT32 )
            flash_erase( key_value_int32, SECTOR_NUM );
        if( KEY_VALUE_STRINGS )
            flash_erase( key_value_string, SECTOR_NUM );
        if( KEY_VALUE_BACKUP )
            flash_erase( key_value_backup, SECTOR_NUM );
    #endif
    
    uint32_t uint32_flag = 0;
    uint32_t strings_flag = 0;
    
    if( KEY_VALUE_INT32 ){
        get_key_value( "UINT32_INIT_FLAG", UINT32, ( uint8_t *)&uint32_flag );
        if( uint32_flag != UINT32_INIT_FLAG ){
            flash_erase( KEY_VALUE_INT32, SECTOR_NUM );
            uint32_flag = UINT32_INIT_FLAG;
            set_key_value( "UINT32_INIT_FLAG", UINT32, ( uint8_t *)&uint32_flag );
        }
    }

    if( KEY_VALUE_STRINGS ){
        get_key_value( "STRINGS_INIT_FLAG", STRINGS, ( uint8_t *)&strings_flag );
        if( strings_flag == 0 || memcmp( (char *)strings_flag, STRINGS_INIT_FLAG, strlen( STRINGS_INIT_FLAG ) ) != 0 ){
            flash_erase( KEY_VALUE_STRINGS, SECTOR_NUM );
            set_key_value( "STRINGS_INIT_FLAG", STRINGS, ( uint8_t *)STRINGS_INIT_FLAG );
        }
    }

    if( KEY_VALUE_INT32 != 0 ){                         //Determine whether the address is valid or not, within the chip address range. 
        if( get_backup_flag( UINT32 ) ){
            move_key_value_back( UINT32 );
        }
    }
    
    if( KEY_VALUE_STRINGS != 0 ){
        if( get_backup_flag( STRINGS ) ){
            move_key_value_back( STRINGS );
        }
    }
    
    if( KEY_VALUE_BACKUP != 0 ){
        backup_flag( UINT32, false );
    }
}

void key_value_test( void ){
    volatile bool flag = true;
    uint32_t i = 0;
    uint32_t j = 0;
    for( i = 0; i < 111111; i++ ){
        if( set_key_value( "key_value_test", UINT32, ( uint8_t * )( &i )) ){
            if( get_key_value( "key_value_test", UINT32, ( uint8_t * )( &j )) && j == i ){
                KEY_VALUE_INFO( " %d\r\n", j );
            }else{
                while( flag );
            }
        }else{
            while( flag );
        }
    }

    uint32_t test_string = 0;
    uint8_t my_string_test[ 16 ] = "";
    for( uint32_t i = 0; i < 111111; i++ ){
        memset( my_string_test, 0, 16 );
        sprintf( (char *)my_string_test, "%d\r\n", i );
        if( set_key_value( "my_string_test", STRINGS, my_string_test ) ){
            if( get_key_value( "my_string_test", STRINGS, (uint8_t *)(&test_string) ) ){
                volatile uint32_t test  = atoi( (char *)test_string );
                if( test == i ){
                    KEY_VALUE_INFO( " %s", (( uint8_t * )test_string) );
                }else{
                    while( flag );
                }
            }else{
                while( flag );
            }
        }else{
            while( flag );
        }
    }
}

uint32_t* __find_key( uint32_t key, enum TYPE type ){
    
    //find string value
    if( type == UINT32 ){
        uint32_t *address = (uint32_t *)KEY_VALUE_INT32;
        for( uint16_t i = 0; i < ( KEY_VALUE_MAX_SIZE / 8 - 1 ); i ++ ){
            if( key == *( address + i * 2 ) ){
                return ( address + i * 2 );
            }
        }
    }else if( type == STRINGS ){
        uint32_t *address = (uint32_t *)KEY_VALUE_STRINGS;
        for( uint16_t i = 0; i < ( KEY_VALUE_MAX_SIZE / 4 - 2 ); i ++ ){
            
            if( key == ERASURE_STATE ){
                if( key == *( address + i ) ){
                    if( i == 0 || (*( address + i - 1 ) & 0xff000000) == 0x00000000 || (*( address + i - 1 ) & 0x000000ff ) == 0x00000000 ){
                        return ( address + i );
                    }else{
                        uint32_t variable = 0xffffff00;
                        flash_write( ( uint8_t * )( &variable ), ( uint32_t )(address + i), 4 );
                        return ( address + i + 1 );
                    }
                }
            }else{
                if( STRINGS_HEAD_FLAG == *( address + i ) ){
                    if( key == *( address + i + 1 ) ){
                        return ( address + i + 1 );
                    }
                }
            }
        }
    }
    return NULL;
}

uint32_t* __find_real_key( uint32_t key, enum TYPE type ){
    uint32_t *realaddress = NULL;
    //find string value
    if( type == UINT32 ){
        uint32_t *address = (uint32_t *)KEY_VALUE_INT32;
        for( uint16_t i = 0; i < ( KEY_VALUE_MAX_SIZE / 8 - 1 ); i ++ ){
            if( key == *( address + i * 2 ) ){
                realaddress = ( address + i * 2 );
            }else if( *( address + i * 2 ) == ERASURE_STATE ){
                break;
            }
        }
    }else if( type == STRINGS ){
        uint32_t *address = (uint32_t *)KEY_VALUE_STRINGS;
        for( uint16_t i = 0; i < ( KEY_VALUE_MAX_SIZE / 4 - 2 ); i ++ ){
            if( STRINGS_HEAD_FLAG == *( address + i ) ){
                if( key == *( address + i + 1 ) ){
                    realaddress = ( address + i + 1 );
                }
            }else if( *( address + i ) == ERASURE_STATE ){
                break;
            }
        }
    }
    return realaddress;
}

bool backup_flag( enum TYPE type, bool stat ){
    
    uint32_t backup_success = 0;
    if( stat ){
        if( type == UINT32 ){
            backup_success = UINT32_FLAG;
        }else if( type == STRINGS ){
            backup_success = STRINGS_FLAG;
        }
    }else{
        backup_success = BACKUP_FLAG_CLEAR;
    }
    stat = false;
    if( flash_write( (const uint8_t *)( &backup_success ), (uint32_t)( KEY_VALUE_BACKUP + KEY_VALUE_MAX_SIZE - 4 ), 4 ) ){
        stat = true;
    }
    return stat;
}

uint8_t get_backup_flag( enum TYPE type ){
    bool stat = false;
    if( type == UINT32 ){
        if( *(uint32_t *)( KEY_VALUE_BACKUP + KEY_VALUE_MAX_SIZE - 4 ) == UINT32_FLAG ){
            stat = true;
        }
    }else if( type == STRINGS ){
        if( *(uint32_t *)( KEY_VALUE_BACKUP + KEY_VALUE_MAX_SIZE - 4 ) == STRINGS_FLAG ){
            stat = true;
        }
    }
    return stat;
}

bool move_key_value_back( enum TYPE type ){
    bool stat = false;
    if( type == UINT32 ){
        
        if( flash_erase( KEY_VALUE_INT32, SECTOR_NUM ) == false ){
            return false;
        }
        
        #if 0
            uint32_t *address = (uint32_t *)KEY_VALUE_BACKUP;
            uint16_t i = 0;
            while( *( address + i ) != ERASURE_STATE ){
                if( !flash_write( (const uint8_t *)( address + i ), (uint32_t)( KEY_VALUE_INT32 + i * 4 ), 4 ) ){
                    return false;
                }
                i++;
            }
            stat = true;
        #else
            if( flash_write( (const uint8_t *)( KEY_VALUE_BACKUP ), (uint32_t)( KEY_VALUE_INT32 ), KEY_VALUE_MAX_SIZE - 8 ) ){
                stat = true;
            }
        #endif
        
    }else if( type == STRINGS ){
        
        if( flash_erase( KEY_VALUE_STRINGS, SECTOR_NUM ) == false ){
            return false;
        }

        #if 0
            uint32_t *address = (uint32_t *)KEY_VALUE_BACKUP;
            uint16_t i = 0;
            while( *( address + i ) != ERASURE_STATE ){//stm32l151 serial
                if( !flash_write( (const uint8_t *)( address + i ), (uint32_t)( KEY_VALUE_STRINGS + i * 4 ), 4 ) ){
                    return false;
                }
                i++;
            }
            stat = true;
        #else
            if( flash_write( (const uint8_t *)( KEY_VALUE_BACKUP ), (uint32_t)( KEY_VALUE_STRINGS ), KEY_VALUE_MAX_SIZE - 8 ) ){
                stat = true;
            }
        #endif
        
    }else{
        return stat;
    }
    
    if( stat ){
        backup_flag( type, false );
    }
    
    return stat;
}

bool move_key_value( enum TYPE type ){
    
    if( flash_erase( KEY_VALUE_BACKUP, SECTOR_NUM ) == false ){
        return false;
    }
    
    if( type == UINT32 ){
        
        uint32_t *address = (uint32_t *)KEY_VALUE_INT32;
        for( uint16_t i = 0, j = 0; i < ( KEY_VALUE_MAX_SIZE / 8 - 1 ); i ++ ){
			if( FILL_STATE != *( address + i * 2 ) ){
                if( ERASURE_STATE == *( address + i * 2 ) ){
                    break;
                }
				if( flash_write( (const uint8_t *)(address + i * 2), (uint32_t)( KEY_VALUE_BACKUP + j * 2 * 4 ), 8 ) ){
                    j++;
                }else{
                    return false;
                }
			}
        }
        
        backup_flag( type, true );

        if( move_key_value_back( type ) ){
            return true;
        }
        
		return false;
        
    }else if( type == STRINGS ){//BUG
        
        uint32_t *address = (uint32_t *)KEY_VALUE_STRINGS;
        
//        #if !defined _STM32L_
//            bool tailed = false;
//            uint8_t count = 0;
//        #endif
        
        for( uint16_t i = 0, j = 0; i < ( KEY_VALUE_MAX_SIZE / 4 ); i ++ ){
            if( STRINGS_HEAD_FLAG == *( address + i ) ){
                while( (*( address + i ) & 0xff000000) != 0x00000000 && (*( address + i ) & 0x000000ff ) != 0x00000000 && i < ( KEY_VALUE_MAX_SIZE / 4 ) ){
                    flash_write( (const uint8_t *)(address + i ), (uint32_t)( KEY_VALUE_BACKUP + j * 4 ), 4 );
                    i++;
                    j++;
                }
                flash_write( (const uint8_t *)(address + i ), (uint32_t)( KEY_VALUE_BACKUP + j * 4 ), 4 );
                j++;
            }
                
//            #if defined _STM32L_
//                if( STRINGS_HEAD_FLAG == *( address + i ) ){
//                    while( (*( address + i ) & 0xff000000) != 0x00000000 && (*( address + i ) & 0x000000ff ) != 0x00000000 && i < ( KEY_VALUE_MAX_SIZE / 4 ) ){
//                        flash_write( (const uint8_t *)(address + i ), (uint32_t)( KEY_VALUE_BACKUP + j * 4 ), 4 );
//                        i++;
//                        j++;
//                    }
//                    flash_write( (const uint8_t *)(address + i ), (uint32_t)( KEY_VALUE_BACKUP + j * 4 ), 4 );
//                    j++;
//                }
//            #else                
//                if( FILL_STATE != *( address + i ) || tailed ){
//                    if( tailed == false && ERASURE_STATE == *( address + i ) ){
//                        break;
//                    }
//                    
//                    tailed = true;
//                    count++;
//                    
//                    if( count > 2 && ( *( address + i ) & 0xff000000) == 0x00000000 ){
//                        tailed = false;
//                        count = 0;
//                    }
//                    
//                    if( flash_write( (const uint8_t *)(address + i ), (uint32_t)( KEY_VALUE_BACKUP + j * 4 ), 4 ) ){
//                        j++;
//                    }else{
//                        return false;
//                    }
//                }
//            #endif
            
        }

        backup_flag( type, true );
        
        if( move_key_value_back( type ) ){
            return true;
        }
        return false;
    }else{
        return false;
    }
}

/************************Simple transplantable key value****************************/

//Conflict situation

int aphash( char* str ){
      
    int hash = 0;

    uint16_t len = strlen( str );
    if( len > 64 ){
      KEY_VALUE_INFO( "aphash String over 64 bytes\r\n");
    }
    for (int i = 0; i < len && len <= 64; i++) {
     if ((i & 1) == 0) {
        hash ^= (hash << 7) ^ ( str[i] ) ^ (hash >> 3);
     } else {
        hash ^= ~((hash << 11) ^ ( str[i] ) ^ (hash >> 5));
     }
    }

    return hash & 0x7FFFFFFF;
}

//Hash conflict check, function check all string key

bool check_repetition( uint32_t *array, uint8_t count ){

    for( uint8_t i = 0; i < count && count < 255; i ++ ){
        for( uint8_t j = i+1; j < count; j ++ ){
            if( array[i] == array[j] ){
                    return true;
            }
        }
    }
    return false;
}

// check_hash_conflict( 5, "liang", "zhang", "gan", "hao", "liu" );
void check_hash_conflict( int count,...){
    
    if( count >= 200 ){
        KEY_VALUE_INFO( "check_hash_conflict--The number is greater than 255\r\n");
        return;
    }
    
    va_list ap;
    char *s;
    
    #if SYS
        uint32_t *array = NULL;
        array = (uint32_t *)pvPortMalloc( count * sizeof(uint32_t) );
        if( array == NULL ){
            KEY_VALUE_INFO( "Allocated memory failure\r\n");
            return;
        }
    #else
        uint32_t array[ 200 ] = { 0 };//Stack overflow may be produced
    #endif

    va_start(ap, count);
    
    for( uint8_t i = 0; i < count ; i ++ ){
        s = va_arg( ap, char*);
        array[i] = aphash( s );
    }
    
    va_end(ap);
    
    if ( check_repetition( array, count) ){
        KEY_VALUE_INFO( "Duplication of data\r\n");
        while( true );
    }else{
        KEY_VALUE_INFO( "No Duplication of data\r\n");
    }
    #if SYS
        vPortFree( array );
    #endif
}

/**********************************************************************************************/
//UINT32        VALUE
//STRINGS       POINTER
bool get_key_value( char *key, enum TYPE type , uint8_t *value ){
    
    bool stat = false;
    
    #if SYS
        static int8_t local_flag = 0;
        if( local_flag == 0 ){
            xSemaphoreTake( key_value_SemaphoreHandle, portMAX_DELAY );
        }
        local_flag ++;
    #endif
    
    int hash = aphash( key );//Possible strings generate hash conflicts

    //find string value
    *((uint32_t *)value) = 0x00;
    uint32_t* key_address = __find_real_key( hash, type );
    
    if( key_address ){
        if( type == UINT32 ){
            if( KEY_VALUE_INT32 == 0x00 ){
                KEY_VALUE_INFO("GET:UINT32 No initialization\r\n");
                while( true );
            }
            *((uint32_t *)value) = *( key_address + 1 );
        }else if( type == STRINGS ){
            if( KEY_VALUE_STRINGS == 0x00 ){
                KEY_VALUE_INFO("GET:STRINGS No initialization\r\n");
                while( true );
            }
            *((uint32_t *)value) = ( uint32_t )( key_address + 1 );
        }
        stat = true;
    }
    
    #if SYS
        local_flag--;
        if( local_flag == 0 ){
            xSemaphoreGive( key_value_SemaphoreHandle );
        }
    #endif
    
    return stat;
}

bool set_key_value( char *key, enum TYPE type, uint8_t *value ){
    
    #if SYS
        static bool key_set_flag = true;
        if( key_set_flag ){
            xSemaphoreTake( key_value_SemaphoreHandle, portMAX_DELAY);
        }
    #endif
    
    bool stat = false;
    int hash = aphash( key );//Possible strings generate hash conflicts

    //find string value
    if( type == UINT32 ){
        
        if( KEY_VALUE_INT32 == 0x00 || KEY_VALUE_BACKUP ==0x00 ){
            KEY_VALUE_INFO("UINT32 No initialization\r\n");
            while( true );
        }
        
        uint32_t* addr = NULL;
        static uint8_t cycleCount = 0;
        int32_rewrite:
        
        addr = __find_key( ERASURE_STATE, type );
        
        if( addr ){
            cycleCount = 0;
            //write real value
			flash_write( (const uint8_t *)(&hash), (uint32_t)( addr ), 4);
			flash_write( (const uint8_t *)(value), (uint32_t)( addr + 1 ), 4);
            volatile bool flag = true;
            
			//compare real value
			if( *(addr) == hash && *((addr + 1) ) == *((uint32_t *)value) && flag ){
                stat = true;
			}else{
                //erase
                #if SYS
                    key_set_flag = false;
                #endif
                uint32_t variable = FILL_STATE;
                flash_write( (const uint8_t *)(&variable), (uint32_t)( addr ), 4);           //flash write 0
                flash_write( (const uint8_t *)(&variable), (uint32_t)( addr + 1 ), 4);       //+4
                rewrite_times ++;
                if( rewrite_times > 3 ){
                    rewrite_times = 0;
                    #if SYS
                        xSemaphoreGive( key_value_SemaphoreHandle );
                    #endif
                    return false;
                }
                if( set_key_value( key, type, value ) ){
                    stat = true;
                }else{
                    stat = false;
                }
                #if SYS
                    key_set_flag = true;
                #endif
                rewrite_times = 0;
                return stat;
            }
            uint32_t* key_address = NULL;
            UINT32_CHECK:
            key_address = __find_key( hash, type );
            if( key_address &&  addr != key_address ){//( *(key_address + 1) != *( uint32_t *)(value)) ){
                uint32_t variable = FILL_STATE;
                flash_write( (const uint8_t *)(&variable), (uint32_t)( key_address ), 4);           //flash write 0
                flash_write( (const uint8_t *)(&variable), (uint32_t)( key_address + 1 ), 4);       //+4
                goto UINT32_CHECK;
            }
        }else{

            //write to backup
			if( move_key_value( type ) == false ){
                KEY_VALUE_INFO("UINT32 write to backup failed\r\n");
				goto exit;
			}
			
            if( cycleCount ++ > 5 ){
                cycleCount = 0;
                KEY_VALUE_INFO( "UINT32 set_key_value backup failed\r\n" );
                goto exit;
            }
            goto int32_rewrite;
        }
    }else if( type == STRINGS ){//UINT32 exception preservation; STRINGS No exception preservation
        
        if( KEY_VALUE_STRINGS == 0x00 || KEY_VALUE_BACKUP ==0x00 ){
            KEY_VALUE_INFO("STRINGS No initialization\r\n");
            while( true );
        }
        
        static uint8_t cycleCount = 0;
        uint32_t* addr = NULL;
        uint16_t len  = strlen( ( char *)value );
        if( len > 256 ){
            KEY_VALUE_INFO( "key_value String over 256 bytes\r\n" );
            return false;
        }
        strings_rewrite:
        addr = __find_key( ERASURE_STATE, type );
        if( addr && ( uint32_t )( addr + 2 + len / 4 + 1 ) < ( KEY_VALUE_STRINGS + KEY_VALUE_MAX_SIZE ) ){
            cycleCount = 0;
            uint32_t variable = 0xffffff00;
            if( len % 4 == 0x00 ){
                flash_write( ( uint8_t * )( &variable ), ( uint32_t )( addr + 2 + len / 4 ), 4 );
            }
            variable = STRINGS_HEAD_FLAG;
            flash_write( (uint8_t *)(&variable), (uint32_t)( addr ), 4);
            flash_write( (uint8_t *)(&hash), (uint32_t)( addr + 1 ), 4);
            flash_write( value, (uint32_t)( addr + 2 ), len );

            if( *(addr) == STRINGS_HEAD_FLAG && *(addr+1) == hash && memcmp( value, addr + 2, len ) == 0x00 ){//bug
                stat = true;
            }else{
                //erase
                #if SYS
                    key_set_flag = false;
                #endif
                uint32_t variable = FILL_STATE;
                flash_write( (const uint8_t *)(&variable), (uint32_t)( addr ), 4);           //flash write 0
                flash_write( (const uint8_t *)(&variable), (uint32_t)( addr + 1 ), 4);       //+4
                rewrite_times ++;
                if( rewrite_times > 3 ){
                    rewrite_times = 0;
                    #if SYS
                        xSemaphoreGive( key_value_SemaphoreHandle );
                    #endif
                    return false;
                }
                if( set_key_value( key, type, value ) ){
                    stat = true;
                }else{
                    stat = false;
                }
                #if SYS
                    key_set_flag = true;
                #endif
                rewrite_times = 0;
                return stat;
            }
            volatile uint32_t* key_address = NULL;
            STRINGS_CHECK:
            
            key_address = __find_key( hash, type );
            
            if( key_address && addr != (key_address-1) ){   //memcmp( (uint8_t *)(key_address+1), value, len ) ){
                uint16_t i = 1;
                uint32_t variable = FILL_STATE;
                flash_write( (const uint8_t *)(&variable), (uint32_t)( key_address - 1 ), 4);
                flash_write( (const uint8_t *)(&variable), (uint32_t)( key_address ), 4);
                
                do{
                    if( (*( key_address + i ) & 0xff000000) == 0x00000000 || (*( key_address + i ) & 0x000000ff ) == 0x00000000 ){
                        flash_write( (const uint8_t *)(&variable), (uint32_t)( key_address + i ), 4);
                        break;
                    }
                    flash_write( (const uint8_t *)(&variable), (uint32_t)( key_address + i++ ), 4);
                }while( ( uint32_t )( key_address + i ) < ( KEY_VALUE_STRINGS + KEY_VALUE_MAX_SIZE ) );
                
                if( ( uint32_t )( key_address + i ) >= ( KEY_VALUE_STRINGS + KEY_VALUE_MAX_SIZE ) ){
                    KEY_VALUE_INFO( "set_key_value Abnormality error\r\n" );
                    goto exit;
                }else{
                    goto STRINGS_CHECK;
                }
            }
            
        }else{
            
            //write to backup
			if( move_key_value( type ) == false ){
                KEY_VALUE_INFO("STRINGS write to backup failed\r\n");
				goto exit;
			}
            
            if( cycleCount ++ > 5 ){
                cycleCount = 0;
                KEY_VALUE_INFO( "STRINGS set_key_value backup failed\r\n" );
                goto exit;
            }
            goto strings_rewrite;
        }
    }
    
    exit:
    
    #if SYS
        xSemaphoreGive( key_value_SemaphoreHandle );
    #endif
    
    return stat;
}
