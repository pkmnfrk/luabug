#include "lua.h"
#include "lauxlib.h"


int print(lua_State* lua)
{
    for(int i = 1; i <= lua_gettop(lua); i++)
    {
        const char* str = lua_tostring(lua, i);
        if(i > 1)
          printf(" ");
        printf("%s", str);
    }
    printf("\n");
    return 0;
}

int main()
{
    lua_State* lua = luaL_newstate();
    lua_register(lua, "print", print);
    luaL_dostring(lua, "test = \"foo\"");
    luaL_loadstring(lua, "print(test)");

    printf("Stack: %d\n", lua_gettop(lua));

    luaL_dostring(lua, "return { test = \"bar\", print = print }");
    lua_setupvalue(lua, -2, 1);
    lua_call(lua, 0, 0);

    return 0;
}