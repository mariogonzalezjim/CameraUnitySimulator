#ifdef __cplusplus
extern "C" {
#endif
#define CREARCAMARA 1
#define CERRARPROGRAMA 9
#define CONECTSERVIDOR 0
#define LOADCAMERASFROMFILE 4
#define GETMINIMAP 2
#define STARTSTREAM 5
#define STOPSTREAM 6
#define CHANGEFORMAT 3
#define CHANGEFRAMERATE 7

void mostrarMenuPrincipal();
int parsearMensajeMenu(char *mensaje);
#ifdef __cplusplus
}
#endif
