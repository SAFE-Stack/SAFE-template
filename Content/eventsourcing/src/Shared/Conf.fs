
namespace Shared
open FSharp.Core

module Conf =
    type StorageType = Postgres | Memory

    let storageType = Postgres

    let isTestEnv = true
    // atm there is only one db, but here I point out that we need different rights in
    // dev/test and in prod respectively.
    // For instance in dev/test we want to give rights to delete events and snapshots
    // At the opposite: in prod db we need to ensure append only mode.
    // We may at most need to delete the snapshots after deploying a new version of the root type (see readme)
    // as long as we respect the substitution principle between versions
    let connectionString =
        if isTestEnv then
            "Server=127.0.0.1;"+
            "Database=safe;" +
            "User Id=safe;"+
            "Password=safe;"
        else
        // create an appropriate db here
            "Server=127.0.0.1;"+
            "Database=safe;" +
            "User Id=safe;"+
            "Password=safe;"

    let intervalBetweenSnapshots = 5

    let cacheSize = 13

