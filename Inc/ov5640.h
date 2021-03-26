#ifndef _OV5640_H
#define _OV5640_H
#include "stm32f4xx_hal.h"
#include "sccb.h"

#define OV5640_PWDN_0  	{HAL_GPIO_WritePin(GPIOA, GPIO_PIN_7, GPIO_PIN_RESET);}
#define OV5640_RST_0  	{HAL_GPIO_WritePin(GPIOC, GPIO_PIN_4, GPIO_PIN_RESET);}
#define OV5640_LED_light_0  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_8, GPIO_PIN_RESET);}
#define OV5640_PWDN_1  	{HAL_GPIO_WritePin(GPIOA, GPIO_PIN_7, GPIO_PIN_SET);}
#define OV5640_RST_1  	{HAL_GPIO_WritePin(GPIOC, GPIO_PIN_4, GPIO_PIN_SET);}
#define OV5640_LED_light_1  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_8, GPIO_PIN_SET);}

#define OV5640_ID               0X5640
 

#define OV5640_ADDR        		0X78

#define OV5640_CHIPIDH          0X300A
#define OV5640_CHIPIDL          0X300B
			
uint8_t OV5640_WR_Reg(uint16_t reg,uint8_t data);
uint8_t OV5640_RD_Reg(uint16_t reg);
void OV5640_PWDN_Set(uint8_t sta);
uint8_t OV5640_Init(void);  
void OV5640_JPEG_Mode(void);
void OV5640_RGB565_Mode(void);
void OV5640_Exposure(uint8_t exposure);
void OV5640_Light_Mode(uint8_t mode);
void OV5640_Color_Saturation(uint8_t sat);
void OV5640_Brightness(uint8_t bright);
void OV5640_Contrast(uint8_t contrast);
void OV5640_Sharpness(uint8_t sharp);
void OV5640_Special_Effects(uint8_t eft);
void OV5640_Test_Pattern(uint8_t mode);
void OV5640_Flash_Ctrl(uint8_t sw);
uint8_t OV5640_OutSize_Set(uint16_t offx,uint16_t offy,uint16_t width,uint16_t height);
uint8_t OV5640_ImageWin_Set(uint16_t offx,uint16_t offy,uint16_t width,uint16_t height); 
uint8_t OV5640_Focus_Init(void);
uint8_t OV5640_Focus_Single(void);
uint8_t OV5640_Focus_Constant(void);
#endif
