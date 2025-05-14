using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT; // Необходимо для MonoPInvokeCallback на некоторых платформах

public class SimpleSQLite
{
    private const string SQLiteLib = "sqlite3"; // Имя библиотеки (без расширения)

    [DllImport(SQLiteLib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int sqlite3_open(string filename, out IntPtr db);

    [DllImport(SQLiteLib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int sqlite3_close(IntPtr db);

    [DllImport(SQLiteLib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int sqlite3_exec(IntPtr db, string sql, IntPtr callback, IntPtr firstArg, out IntPtr errmsg);

    [DllImport(SQLiteLib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr sqlite3_errmsg(IntPtr db);

    public static IntPtr dbConnection;

    private delegate int SQLiteCallback(IntPtr userData, int columnCount, IntPtr columnValues, IntPtr columnNames);
    private static SQLiteCallback callbackDelegate = new SQLiteCallback(RowCallback);

    public static bool Open(string databasePath)
    {
        if (sqlite3_open(databasePath, out dbConnection) != 0)
        {
            Debug.LogError($"Ошибка открытия базы данных: {Marshal.PtrToStringAnsi(sqlite3_errmsg(dbConnection))}");
            return false;
        }
        return true;
    }

    public static void Close()
    {
        if (dbConnection != IntPtr.Zero)
        {
            sqlite3_close(dbConnection);
            dbConnection = IntPtr.Zero;
        }
    }

    public static List<string[]> ExecuteQuery(string query)
    {
        List<string[]> results = new List<string[]>();
        IntPtr errorMsg = IntPtr.Zero;

        sqlite3_exec(dbConnection, query, Marshal.GetFunctionPointerForDelegate(callbackDelegate), GCHandle.ToIntPtr(GCHandle.Alloc(results)), out errorMsg);

        if (errorMsg != IntPtr.Zero)
        {
            Debug.LogError($"Ошибка выполнения запроса: {Marshal.PtrToStringAnsi(errorMsg)}");
            Marshal.FreeHGlobal(errorMsg);
        }

        return results;
    }

    [MonoPInvokeCallback(typeof(SQLiteCallback))]
    private static int RowCallback(IntPtr userData, int columnCount, IntPtr columnValues, IntPtr columnNames)
    {
        List<string[]> rows = (List<string[]>)GCHandle.FromIntPtr(userData).Target;
        string[] row = new string[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            row[i] = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(columnValues, i * IntPtr.Size));
        }
        rows.Add(row);
        return 0;
    }
}