## docker-compose.yml
#version: "3.9"
#
#services:
  #db:
    #image: postgres:17
    #restart: always
    #shm_size: 128mb
    #environment:
      #POSTGRES_PASSWORD: postgres
      #POSTGRES_USER: postgres
      #POSTGRES_DB: missdb
    #ports:
      #- "60326:5432" # или "5432:5432" если порт свободен
    #volumes:
      #- pgdata:/var/lib/postgresql/data
#
  #adminer:
    #image: adminer
    #restart: always
    #depends_on:
      #- db
    #ports:
      #- "8080:8080"
#
#volumes:
  #pgdata: