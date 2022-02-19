<!-- <img width="150" align="right" title="cpu icon" src="./resources/cpu.png" alt_text="[Cpu icons created by Freepik - Flaticon](https://www.flaticon.com/premium-icon/cpu_707374?related_id=707576)"></img> -->

# Cache Simulator
<p align="justify"><b>Cache Simulator</b> was created for a <i>Computer Architecture</i> course project, as taught at the Faculty of Electrical Engineering Banja Luka. The project has been sence expanded and updated.</p>

## Table of contents
- [Cache Simulator](#cache-simulator)
  - [Table of contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Replacement policy](#replacement-policy)
    - [LRU (Least-recently used)](#lru-least-recently-used)
  - [Associativity (placment policy)](#associativity-placment-policy)
    - [Direct-mapped cache](#direct-mapped-cache)
  - [Ram memory](#ram-memory)
  - [Trace file](#trace-file)
  - [To-Do List](#to-do-list)

## Introduction
<p align="justify"><b>Cache Simulator</b> is simulator implemented in C#. It supports directly mapped, N-way set associative or fully associative cache memory. It also allows LRU (Least Recently Used), Bélády's or Random replacement policy. The cache is simulated inside of the computer's RAM memory and the simulated RAM is stored on the computer's NTFS file system. The cache simulator first checks the users inputs. </p>

## Replacement policy
<p align="justify">To make room for the new entry on a cache miss, the cache may have to evict one of the existing entries.</p>

### LRU (Least-recently used)
<p align="justify">Algorithm requeres keeping track of what was used when. This was accomplished by using an <i>age bit</i> for cache-lines and then track the LRU cache-line based on the age-bits. </p>

```C#
public int Age { get; set; } = 0;
```
<p align="justify">In this implementation, every time a cache-line is used, the age of all other cache-lines in the set changes.</p>

```C#
for (var i = limit; i < limit + Associativity; ++i)
{
    ++CacheEntries[i].Age;
}
```

## Associativity (placment policy)
<p align="justify">The placment policy decides where in the cache a copy of a particular entry of main memory will go. If the placment policy is free to choose any entry in the cache to hold the copy, the cache is <i>fully associattive</i>. At the other extreme, if each entry in main memory can go in just one place in the cache, the cache is <i>directly mapped</i>. The comprimise between the two extreems, in which each entry in main memory can go to any of N places in the cache are described as <i>N-way set associative</i>.<br>Choosing the right value of associativity involves a trade-off. If there is eight palces to which the placment policy have mapped memory location, then to check if that location is in the cache, eight cache entries must be searched.</p>

### Direct-mapped cache
<p align="justify">It doesn't have a placment policy as such, since there is no choice of which cache entry's content to evict. This means that if two locatios map to the same entry, they continually knock each outher out.</p>

## Ram memory

## Trace file

## To-Do List
- [ ] Add L1-I cache memory.
  - [ ] Add type '<b>I</b>' instruction to trace file.
- [ ] Add L2 cache memory (shared cache for all cores).
- [ ] Implement FIFO replacement policy.
- [ ] Implement LIFO replacement policy.
- [ ] Implement TLRU replacement policy.
- [ ] Implement LFU replacement policy.
- [ ] Implement LFUDA replacement policy.