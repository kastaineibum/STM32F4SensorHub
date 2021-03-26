/**
  * Board Designer				3mn.net demotree.vip  
	* All code between comment pairs USER CODE BEGIN and USER CODE END 
	* are written by Board Designer.
	* 
	* Copyright (c) 2019 Beijing Lizishu Tech Co.,Ltd.
  * All rights reserved.
  */
	
	
#include "dht11.h"
#include "dwt_stm32_delay.h"
	  
void DHT11_IN(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};

  __HAL_RCC_GPIOB_CLK_ENABLE();

  GPIO_InitStruct.Pin = GPIO_PIN_3;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}

void DHT11_OUT(void)
{
	GPIO_InitTypeDef GPIO_InitStruct = {0};

  __HAL_RCC_GPIOB_CLK_ENABLE();

  GPIO_InitStruct.Pin = GPIO_PIN_3;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}

void DHT11_Init(void)
{					     
  GPIO_InitTypeDef GPIO_InitStruct = {0};
	
  __HAL_RCC_GPIOB_CLK_ENABLE();
	
  GPIO_InitStruct.Pin = GPIO_PIN_3;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
 
	DHT11_1;

}

unsigned int DHT11_Read(void)
{
  int i;
  long long val;
  int timeout;
 
  DHT11_0;
  delay_us(18000);
  DHT11_1;
  delay_us(20);
 
  DHT11_IN();
 
  timeout = 5000;
  while( (! READ_DHT11) && (timeout > 0) ) timeout--;
 
  timeout = 5000;
  while( READ_DHT11 && (timeout > 0) ) timeout-- ;
 
#define CHECK_TIME 28
 
  for(i=0;i<40;i++)
  {
	timeout = 5000;
	while( (! READ_DHT11) && (timeout > 0) ) timeout--;
 
	delay_us(CHECK_TIME);
	if ( READ_DHT11 )
	{
	  val=(val<<1)+1;
	} else {
	  val<<=1;
	}
 
	timeout = 5000;
	while( READ_DHT11 && (timeout > 0) ) timeout-- ;
  }
 
  DHT11_OUT();
  DHT11_1;
	
	//printf(" dht11 raw: %08x \r\n",(unsigned int)(val>>8));
 
  if (((val>>32)+(val>>24)+(val>>16)+(val>>8) -val ) & 0xff  ) return 0; 
    else return val>>8;
 
}

uint8_t getHumidity(void)
{
	unsigned int r = 0;
	uint8_t v = 0;
	for(int i=0;i<5&&v==0;i++)
	{
		delay_ms(100);
		r = DHT11_Read();
		v = r>>24;
	}
	return v;
}

uint8_t getTemperature(void)
{
	unsigned int r = 0;
	uint8_t v = 0;
	for(int i=0;i<5&&v==0;i++)
	{
		delay_ms(100);
		r = DHT11_Read();
		v = (r&0x0000ffff)>>8;
	}
	return v;
}

uint32_t getDHTRaw(void)
{
	unsigned int r = 0;
	uint32_t v = 0;
	for(int i=0;i<5&&v==0;i++)
	{
		delay_ms(100);
		r = DHT11_Read();
		v = r;
		//DHT11_Init();
	}
	return v;
}
